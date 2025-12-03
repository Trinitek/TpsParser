using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TpsParser.TypeModel;

namespace TpsParser.Data;

/// <summary>
/// Reads a forward-only stream of rows from a TPS file.
/// </summary>
public class TpsDataReader : DbDataReader
{
    private sealed record ColumnDef
    {
        public FieldDefinition? FieldDef { get; }
        public MemoDefinition? MemoDef { get; }

        public string Name => FieldDef?.Name ?? MemoDef!.Name;

        public ColumnDef(FieldDefinition fieldDef)
        {
            FieldDef = fieldDef ?? throw new ArgumentNullException(nameof(fieldDef));
            MemoDef = null;
        }

        public ColumnDef(MemoDefinition memoDef)
        {
            FieldDef = null;
            MemoDef = memoDef ?? throw new ArgumentNullException(nameof(memoDef));
        }
    }

    private readonly TpsFileConnectionContext _connectionContext;
    private readonly TableDefinition _tableDefinition;
    private readonly int _tableNumber;
    private readonly ImmutableArray<FieldIteratorNode> _fieldIteratorNodes;

    private FrozenDictionary<string, int> NameToOrdinalLookup { get; }
    private ReadOnlyCollection<ColumnDef> ColumnDefinitions { get; }
    private IEnumerator<DataRecordPayload>? DataRecordPayloadEnumerator { get; set; }

    private const int VIRTUAL_FIELDS_TOTAL = 1;
    private const string VIRTUAL_FIELDS_RECORDNUMBER = "__RECORD_NUMBER";
    private const int VIRTUAL_FIELDS_RECORDNUMBER_OFFSET = 0;

    private bool _isDisposed;

    /// <summary>
    /// Instantiates a new reader.
    /// </summary>
    /// <param name="connectionContext"></param>
    /// <param name="tableDefinition"></param>
    /// <param name="tableNumber"></param>
    /// <param name="fieldIteratorNodes"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public TpsDataReader(
        TpsFileConnectionContext connectionContext,
        TableDefinition tableDefinition,
        int tableNumber,
        ImmutableArray<FieldIteratorNode> fieldIteratorNodes)
    {
        _connectionContext = connectionContext ?? throw new ArgumentNullException(nameof(connectionContext));
        _tableDefinition = tableDefinition ?? throw new ArgumentNullException(nameof(tableDefinition));
        _tableNumber = tableNumber;
        _fieldIteratorNodes = fieldIteratorNodes;

        var columnDefinitions = new List<ColumnDef>(
            capacity: fieldIteratorNodes.Length + _tableDefinition.Memos.Length);

        columnDefinitions.AddRange(fieldIteratorNodes.Select(node => new ColumnDef(node.DefinitionPointer.Inner)));
        columnDefinitions.AddRange(_tableDefinition.Memos.Select(memoDef => new ColumnDef(memoDef)));

        ColumnDefinitions = columnDefinitions.AsReadOnly();

        var nameToOrdinalLookup = new Dictionary<string, int>(
            capacity: columnDefinitions.Count + VIRTUAL_FIELDS_TOTAL);

        for (int ordinal = 0; ordinal < columnDefinitions.Count; ordinal++)
        {
            var columnDef = columnDefinitions[ordinal];

            nameToOrdinalLookup.Add(
                key: columnDef.Name,
                value: ordinal);
        }

        nameToOrdinalLookup.Add(
            key: VIRTUAL_FIELDS_RECORDNUMBER,
            value: columnDefinitions.Count + VIRTUAL_FIELDS_RECORDNUMBER_OFFSET);

        NameToOrdinalLookup = nameToOrdinalLookup.ToFrozenDictionary(
            comparer: StringComparer.InvariantCultureIgnoreCase);
    }

    private void AssertNotDisposed()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
    }

    private void AssertOrdinalIsWithinRange(int ordinal)
    {
        const int minOrdinal = 0;
        int maxOrdinal = ColumnDefinitions.Count + VIRTUAL_FIELDS_TOTAL;

        if (ordinal < 0 || ordinal >= (ColumnDefinitions.Count + VIRTUAL_FIELDS_TOTAL))
        {
            throw new IndexOutOfRangeException($"Ordinal out of range: {ordinal}. Expected between {minOrdinal} and {maxOrdinal}.");
        }
    }

    private DataRecordPayload GetCurrentDataRecordPayload()
    {
        if (DataRecordPayloadEnumerator is null)
        {
            throw new InvalidOperationException("Record enumeration has not started yet. Call Read() first.");
        }

        return DataRecordPayloadEnumerator.Current;
    }

    /// <inheritdoc/>
    public override void Close()
    {
        base.Close();

        _isDisposed = true;
    }

    /// <inheritdoc/>
    public override bool HasRows
    {
        get
        {
            AssertNotDisposed();

            _hasRows ??= _connectionContext.TpsFile.GetDataRecordPayloads(table: _tableNumber).Any();

            return _hasRows.Value;
        }
    }
    private bool? _hasRows;

    /// <inheritdoc/>
    public override object this[int i]
    {
        get
        {
            AssertNotDisposed();

            var currentRecord = GetCurrentDataRecordPayload();

            if (i == ColumnDefinitions.Count + VIRTUAL_FIELDS_RECORDNUMBER_OFFSET)
            {
                return (long)currentRecord.RecordNumber;
            }

            if (i < 0 || i >= ColumnDefinitions.Count)
            {
                throw new IndexOutOfRangeException($"No such column index: {i}.");
            }

            if (i < _fieldIteratorNodes.Length)
            {
                var node = _fieldIteratorNodes[i];

                var result = FieldValueReader.GetValue(node, currentRecord);

                return ConvertToClrType(result.Value);
            }

            var memo = _connectionContext.MemoIndexer.GetValue(
                tableDefinition: _tableDefinition,
                tableNumber: _tableNumber,
                owningRecord: currentRecord.RecordNumber,
                definitionIndex: (byte)(i - _fieldIteratorNodes.Length));

            if (memo is TpsTextMemo textMemo)
            {
                return textMemo.ToString(_connectionContext.TpsFile.EncodingOptions.ContentEncoding);
            }
            else if (memo is TpsBlob blob)
            {
                return blob.ToArray();
            }
            else
            {
                return DBNull.Value;
            }
        }
    }

    /// <inheritdoc/>
    public override object this[string name]
    {
        get
        {
            AssertNotDisposed();

            var currentRecord = GetCurrentDataRecordPayload();

            if (string.Equals(name, VIRTUAL_FIELDS_RECORDNUMBER, StringComparison.InvariantCultureIgnoreCase))
            {
                return (long)currentRecord.RecordNumber;
            }

            if (!NameToOrdinalLookup.TryGetValue(name, out int ordinal))
            {
                throw new IndexOutOfRangeException($"No such column name: '{name}'.");
            }

            if (ordinal < _fieldIteratorNodes.Length)
            {
                var node = _fieldIteratorNodes[ordinal];

                var result = FieldValueReader.GetValue(node, currentRecord);

                return ConvertToClrType(result.Value);
            }

            int memoOrdinal = ordinal - _fieldIteratorNodes.Length;

            var memo = _connectionContext.MemoIndexer.GetValue(
                tableDefinition: _tableDefinition,
                tableNumber: _tableNumber,
                owningRecord: currentRecord.RecordNumber,
                definitionIndex: (byte)memoOrdinal);

            if (memo is TpsTextMemo textMemo)
            {
                return textMemo.ToString(_connectionContext.TpsFile.EncodingOptions.ContentEncoding);
            }
            else if (memo is TpsBlob blob)
            {
                return blob.ToArray();
            }
            else
            {
                return DBNull.Value;
            }
        }
    }

    /// <inheritdoc/>
    public override int Depth => 0;

    /// <inheritdoc/>
    public override bool IsClosed => !_isDisposed;

    /// <inheritdoc/>
    public override int RecordsAffected => -1;

    private int FieldCountCore => ColumnDefinitions.Count + VIRTUAL_FIELDS_TOTAL;

    /// <inheritdoc/>
    public override int FieldCount
    {
        get
        {
            AssertNotDisposed();

            return FieldCountCore;
        }
    }

    /// <inheritdoc/>
    public override bool GetBoolean(int ordinal)
    {
        return (bool)this[ordinal];
    }

    /// <inheritdoc/>
    public override byte GetByte(int ordinal)
    {
        return (byte)this[ordinal];
    }

    /// <inheritdoc/>
    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override char GetChar(int ordinal)
    {
        return (char)this[ordinal];
    }

    /// <inheritdoc/>
    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override string GetDataTypeName(int ordinal)
    {
        AssertOrdinalIsWithinRange(ordinal);

        if (ordinal < _fieldIteratorNodes.Length)
        {
            var fieldDef = _fieldIteratorNodes[ordinal].DefinitionPointer;

            return fieldDef.TypeCode switch
            {
                FieldTypeCode.Byte => "BYTE",
                FieldTypeCode.Short => "SHORT",
                FieldTypeCode.UShort => "USHORT",
                FieldTypeCode.Date => "DATE",
                FieldTypeCode.Time => "TIME",
                FieldTypeCode.Long => "LONG",
                FieldTypeCode.ULong => "ULONG",
                FieldTypeCode.SReal => "SREAL",
                FieldTypeCode.Real => "REAL",
                FieldTypeCode.Decimal => "DECIMAL",
                FieldTypeCode.FString => "STRING",
                FieldTypeCode.CString => "CSTRING",
                FieldTypeCode.PString => "PSTRING",
                FieldTypeCode.Group => "GROUP",
                _ => throw new NotImplementedException($"TypeCode name not implemented for {fieldDef.TypeCode}.")
            };
        }

        if (ordinal < _fieldIteratorNodes.Length + _tableDefinition.Memos.Length)
        {
            int memoIndex = ordinal - _fieldIteratorNodes.Length;

            var memoDef = _tableDefinition.Memos[memoIndex];

            return memoDef.IsTextMemo ? "MEMO" : "BLOB";
        }

        // RECORD_NUMBER
        if (ordinal == FieldCount - 1)
        {
            return string.Empty;
        }

        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override DateTime GetDateTime(int ordinal)
    {
        return (DateTime)this[ordinal];
    }

    /// <inheritdoc/>
    public override decimal GetDecimal(int ordinal)
    {
        return (decimal)this[ordinal];
    }

    /// <inheritdoc/>
    public override double GetDouble(int ordinal)
    {
        return (double)this[ordinal];
    }

    private object ConvertToClrType(IClaObject claObject) =>
        claObject switch
        {
            ClaByte claByte => claByte.ToByte().Value,
            ClaShort claShort => claShort.ToInt16().Value,
            ClaUnsignedShort claUnsignedShort => claUnsignedShort.ToUInt16().Value,
            ClaDate claDate => claDate.ToDateOnly().Value is var d && d.HasValue ? d : DBNull.Value,
            ClaTime claTime => claTime.ToTimeOnly().Value is var t && t.HasValue ? t : DBNull.Value,
            ClaLong claLong => claLong.ToInt32().Value,
            ClaUnsignedLong claUnsignedLong => claUnsignedLong.ToUInt32().Value,
            ClaSingleReal claSingleReal => claSingleReal.ToFloat().Value,
            ClaReal claReal => claReal.ToDouble().Value,
            ClaDecimal claDecimal => claDecimal.ToDecimal(),
            IClaString claString => claString.ToString(_connectionContext.TpsFile.EncodingOptions.ContentEncoding),
            ClaGroup claGroup => claGroup,
            _ => throw new NotImplementedException($"Type conversion not implemented for {claObject?.GetType()?.Name ?? "[null]"}.")
        };

    /// <inheritdoc/>
    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
    public override Type GetFieldType(int ordinal)
    {
        AssertOrdinalIsWithinRange(ordinal);

        if (ordinal == ColumnDefinitions.Count + VIRTUAL_FIELDS_RECORDNUMBER_OFFSET)
        {
            //This will be VIRTUAL_FIELDS_RECORDNUMBER
            return typeof(long);
        }
        
        var columnDef = ColumnDefinitions[ordinal];

        if (columnDef.FieldDef is FieldDefinition { } fieldDef)
        {
            return fieldDef.TypeCode switch
            {
                FieldTypeCode.Byte => typeof(byte),
                FieldTypeCode.Short => typeof(short),
                FieldTypeCode.UShort => typeof(ushort),
                FieldTypeCode.Date => typeof(DateOnly?),
                FieldTypeCode.Time => typeof(TimeOnly?),
                FieldTypeCode.Long => typeof(long),
                FieldTypeCode.ULong => typeof(ulong),
                FieldTypeCode.SReal => typeof(double),
                FieldTypeCode.Real => typeof(double),
                FieldTypeCode.Decimal => typeof(decimal),
                FieldTypeCode.FString => typeof(string),
                FieldTypeCode.CString => typeof(string),
                FieldTypeCode.PString => typeof(string),
                FieldTypeCode.Group => typeof(ClaGroup),
                _ => throw new NotImplementedException($"Type conversion for FieldDefinition of type '{fieldDef.TypeCode}' is not implemented."),
            };
        }

        if (columnDef.MemoDef is MemoDefinition { } memoDef)
        {
            if (memoDef.IsBlob)
            {
                return typeof(byte[]);
            }

            if (memoDef.IsTextMemo)
            {
                return typeof(string);
            }

            throw new NotImplementedException($"Type conversion for this particular MemoDefinition is not implemented.");
        }

        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override float GetFloat(int ordinal)
    {
        return (float)this[ordinal];
    }

    /// <inheritdoc/>
    public override Guid GetGuid(int ordinal)
    {
        return (Guid)this[ordinal];
    }

    /// <inheritdoc/>
    public override short GetInt16(int ordinal)
    {
        return (short)this[ordinal];
    }

    /// <inheritdoc/>
    public override int GetInt32(int ordinal)
    {
        return (int)this[ordinal];
    }

    /// <inheritdoc/>
    public override long GetInt64(int ordinal)
    {
        return (long)this[ordinal];
    }

    /// <inheritdoc/>
    public override string GetName(int ordinal)
    {
        AssertNotDisposed();
        AssertOrdinalIsWithinRange(ordinal);

        if (ordinal == ColumnDefinitions.Count + VIRTUAL_FIELDS_RECORDNUMBER_OFFSET)
        {
            return VIRTUAL_FIELDS_RECORDNUMBER;
        }

        return ColumnDefinitions[ordinal].Name;
    }

    /// <inheritdoc/>
    public override int GetOrdinal(string name)
    {
        AssertNotDisposed();

        if (string.Equals(name, VIRTUAL_FIELDS_RECORDNUMBER, StringComparison.InvariantCultureIgnoreCase))
        {
            return ColumnDefinitions.Count + VIRTUAL_FIELDS_RECORDNUMBER_OFFSET;
        }

        if (!NameToOrdinalLookup.TryGetValue(name, out int ordinal))
        {
            throw new IndexOutOfRangeException($"No such column name: '{name}'.");
        }

        return ordinal;
    }

    /// <inheritdoc/>
    public override DataTable? GetSchemaTable()
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc/>
    public override string GetString(int ordinal)
    {
        return (string)this[ordinal];
    }

    /// <inheritdoc/>
    public override object GetValue(int ordinal)
    {
        return this[ordinal];
    }

    /// <inheritdoc/>
    public override int GetValues(object[] values)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override bool IsDBNull(int ordinal)
    {
        AssertNotDisposed();
        AssertOrdinalIsWithinRange(ordinal);

        // Of the field types, only ClaDate and ClaTime can potentially be DbNull; all others are not.

        if (ordinal < _fieldIteratorNodes.Length || ordinal >= ColumnDefinitions.Count)
        {
            return GetValue(ordinal) is DBNull;
        }

        // MEMOs and BLOBs can also be DbNull.

        int memoOrdinal = ordinal - _fieldIteratorNodes.Length;

        var currentRecord = GetCurrentDataRecordPayload();

        var maybeMemo = _connectionContext.MemoIndexer.GetValue(
            tableDefinition: _tableDefinition,
            tableNumber: _tableNumber,
            owningRecord: currentRecord.RecordNumber,
            definitionIndex: (byte)memoOrdinal);

        return maybeMemo is null;
    }

    /// <inheritdoc/>
    public override bool NextResult()
    {
        return false;
    }

    /// <inheritdoc/>
    public override bool Read()
    {
        AssertNotDisposed();

        if (DataRecordPayloadEnumerator is null)
        {
            _connectionContext.MemoIndexer.EnsureBuiltForTables(_tableNumber);

            var dataRecordPayloads = _connectionContext.TpsFile.GetDataRecordPayloads(
                table: _tableNumber);

            DataRecordPayloadEnumerator = dataRecordPayloads.GetEnumerator();
        }

        bool moveNextResult = DataRecordPayloadEnumerator.MoveNext();

        _hasRows ??= moveNextResult;

        return moveNextResult;
    }

    /// <inheritdoc/>
    public override IEnumerator GetEnumerator()
    {
        throw new NotImplementedException();
    }
}
