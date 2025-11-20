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

    private readonly TpsFile _tpsFile;
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

    public TpsDataReader(
        TpsFile tpsFile,
        TableDefinition tableDefinition,
        int tableNumber,
        ImmutableArray<FieldIteratorNode> fieldIteratorNodes)
    {
        _tpsFile = tpsFile ?? throw new ArgumentNullException(nameof(tpsFile));
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

    public override void Close()
    {
        base.Close();

        _isDisposed = true;
    }

    public override bool HasRows
    {
        get
        {
            AssertNotDisposed();

            _hasRows ??= _tpsFile.GetDataRecordPayloads(table: _tableNumber).Any();

            return _hasRows.Value;
        }
    }
    private bool? _hasRows;


    public override object this[int i]
    {
        get
        {
            AssertNotDisposed();

            var currentRecord = GetCurrentDataRecordPayload();

            if (i == ColumnDefinitions.Count + VIRTUAL_FIELDS_RECORDNUMBER_OFFSET)
            {
                return currentRecord.RecordNumber;
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

            var memos = _tpsFile.GetTpsMemos(
                table: _tableNumber,
                owningRecord: currentRecord.RecordNumber,
                memoDefinitionIndex: (byte)(i - _fieldIteratorNodes.Length));

            var memo = memos.SingleOrDefault();

            if (memo is TpsTextMemo textMemo)
            {
                return textMemo.ToString(_tpsFile.EncodingOptions.ContentEncoding);
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

    public override object this[string name]
    {
        get
        {
            AssertNotDisposed();

            var currentRecord = GetCurrentDataRecordPayload();

            if (string.Equals(name, VIRTUAL_FIELDS_RECORDNUMBER, StringComparison.InvariantCultureIgnoreCase))
            {
                return currentRecord.RecordNumber;
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

            var memos = _tpsFile.GetTpsMemos(
                        table: _tableNumber,
                        owningRecord: currentRecord.RecordNumber,
                        memoDefinitionIndex: (byte)memoOrdinal);

            var memo = memos.SingleOrDefault();

            if (memo is TpsTextMemo textMemo)
            {
                return textMemo.ToString(_tpsFile.EncodingOptions.ContentEncoding);
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

    public override int Depth => 0;

    public override bool IsClosed => !_isDisposed;

    public override int RecordsAffected => 0;

    private int FieldCountCore => ColumnDefinitions.Count + VIRTUAL_FIELDS_TOTAL;

    public override int FieldCount
    {
        get
        {
            AssertNotDisposed();

            return FieldCountCore;
        }
    }

    public override bool GetBoolean(int ordinal)
    {
        return (bool)this[ordinal];
    }

    public override byte GetByte(int ordinal)
    {
        return (byte)this[ordinal];
    }

    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
    {
        throw new NotImplementedException();
    }

    public override char GetChar(int ordinal)
    {
        return (char)this[ordinal];
    }

    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
    {
        throw new NotImplementedException();
    }

    public override string GetDataTypeName(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override DateTime GetDateTime(int ordinal)
    {
        return (DateTime)this[ordinal];
    }

    public override decimal GetDecimal(int ordinal)
    {
        return (decimal)this[ordinal];
    }

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
            ClaDate claDate => claDate.ToDateOnly(),
            ClaTime claTime => claTime.ToTimeOnly(),
            ClaLong claLong => claLong.ToInt32().Value,
            ClaUnsignedLong claUnsignedLong => claUnsignedLong.ToUInt32().Value,
            ClaSingleReal claSingleReal => claSingleReal.ToFloat().Value,
            ClaReal claReal => claReal.ToDouble().Value,
            ClaDecimal claDecimal => claDecimal.ToDecimal(),
            IClaString claString => claString.ToString(_tpsFile.EncodingOptions.ContentEncoding),
            ClaGroup claGroup => claGroup,
            _ => throw new NotImplementedException($"Type conversion not implemented for {claObject?.GetType()?.Name ?? "[null]"}.")
        };

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
                FieldTypeCode.Date => typeof(DateOnly),
                FieldTypeCode.Time => typeof(TimeOnly),
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

    public override float GetFloat(int ordinal)
    {
        return (float)this[ordinal];
    }

    public override Guid GetGuid(int ordinal)
    {
        return (Guid)this[ordinal];
    }

    public override short GetInt16(int ordinal)
    {
        return (short)this[ordinal];
    }

    public override int GetInt32(int ordinal)
    {
        return (int)this[ordinal];
    }

    public override long GetInt64(int ordinal)
    {
        return (long)this[ordinal];
    }

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

    public override DataTable? GetSchemaTable()
    {
        throw new NotSupportedException();
    }

    public override string GetString(int ordinal)
    {
        return (string)this[ordinal];
    }

    public override object GetValue(int ordinal)
    {
        return this[ordinal];
    }

    public override int GetValues(object[] values)
    {
        throw new NotImplementedException();
    }

    public override bool IsDBNull(int ordinal)
    {
        AssertNotDisposed();
        AssertOrdinalIsWithinRange(ordinal);

        // MEMOs and BLOBs can potentially be DbNull; all others are not.

        if (ordinal < _fieldIteratorNodes.Length || ordinal >= ColumnDefinitions.Count)
        {
            return false;
        }

        int memoOrdinal = ordinal - _fieldIteratorNodes.Length;

        var currentRecord = GetCurrentDataRecordPayload();

        var memos = _tpsFile.GetTpsMemos(
                    table: _tableNumber,
                    owningRecord: currentRecord.RecordNumber,
                    memoDefinitionIndex: (byte)memoOrdinal);

        var maybeMemo = memos.FirstOrDefault();

        return maybeMemo is null;
    }

    public override bool NextResult()
    {
        return false;
    }

    public override bool Read()
    {
        AssertNotDisposed();

        if (DataRecordPayloadEnumerator is null)
        {
            var dataRecordPayloads = _tpsFile.GetDataRecordPayloads(
                table: _tableNumber);

            DataRecordPayloadEnumerator = dataRecordPayloads.GetEnumerator();
        }

        bool moveNextResult = DataRecordPayloadEnumerator.MoveNext();

        _hasRows ??= moveNextResult;

        return moveNextResult;
    }

    public override IEnumerator GetEnumerator()
    {
        throw new NotImplementedException();
    }
}
