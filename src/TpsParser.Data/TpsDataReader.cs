using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;
using TpsParser.TypeModel;

namespace TpsParser.Data;

public class TpsDataReader : DbDataReader
{
    private TpsFile _tpsFile;
    private TableDefinition _tableDefinition;
    private int _tableNumber;
    private ImmutableArray<FieldIteratorNode> _fieldIteratorNodes;

    //private Dictionary<int, string> IndexToColumnNames = new();
    //private Dictionary<string, int> ColumnNamesToIndex = new();
    //private HashSet<string> ColumnNames_WithCase = new();
    //private Dictionary<string, string> ColumnNames_ToCase = new();

    private readonly List<(FieldDefinition? FieldDef, MemoDefinition? MemoDef)> _columnDefinitions = [];

    private IEnumerable<DataRecordPayload> _dataRecordPayloads;
    private IEnumerator<DataRecordPayload> _dataRecordPayloadEnumerator;

    private IEnumerable<FieldEnumerationResult>? _currentEnumerationResults = null;

    private const int VIRTUAL_FIELDS_TOTAL = 1;
    private const string VIRTUAL_FIELDS_RECORDNUMBER = "__RECORD_NUMBER";
    private const int VIRTUAL_FIELDS_RECORDNUMBER_OFFSET = 0;

    private bool IsDisposed;

    public override void Close()
    {
        base.Close();
        
        //Parser = null;
        //TableDefinitions.Clear();
        //IndexToColumnNames.Clear();
        //ColumnNamesToIndex.Clear();
        //ColumnNames_WithCase.Clear();
        //ColumnNames_ToCase.Clear();
        _columnDefinitions.Clear();
        //Rows.Clear();

        IsDisposed = true;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
    }

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
    }

    public override bool HasRows
    {
        get
        {
            ThrowIfDisposed();

            return _dataRecordPayloads.Any();
        }
    }

    public override object this[int i]
    {
        get
        {
            ThrowIfDisposed();

            if (i == _fieldIteratorNodes.Length + VIRTUAL_FIELDS_RECORDNUMBER_OFFSET)
            {
                return _dataRecordPayloadEnumerator.Current.RecordNumber;
            }

            if (i < 0 || i >= _columnDefinitions.Count)
            {
                throw new IndexOutOfRangeException($"No such column index: {i}.");
            }

            if (i < _fieldIteratorNodes.Length)
            {
                var node = _fieldIteratorNodes[i];

                var result = FieldValueReader.GetValue(node, _dataRecordPayloadEnumerator.Current);

                return ConvertToClrType(result.Value);
            }

            var memos = _tpsFile.GetTpsMemos(
                table: _tableNumber,
                owningRecord: _dataRecordPayloadEnumerator.Current.RecordNumber,
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
            ThrowIfDisposed();

            if (string.Equals(name, VIRTUAL_FIELDS_RECORDNUMBER, StringComparison.InvariantCultureIgnoreCase))
            {
                return _dataRecordPayloadEnumerator.Current.RecordNumber;
            }

            foreach (var node in _fieldIteratorNodes)
            {
                if (string.Equals(node.DefinitionPointer.Inner.Name, name, StringComparison.InvariantCultureIgnoreCase))
                {
                    var result = FieldValueReader.GetValue(node, _dataRecordPayloadEnumerator.Current);

                    return ConvertToClrType(result.Value);
                }
            }

            for (byte memoIndex = 0; memoIndex < _tableDefinition.Memos.Length; memoIndex++)
            {
                var memoDef = _tableDefinition.Memos[memoIndex];

                if (string.Equals(memoDef.Name, name, StringComparison.InvariantCultureIgnoreCase))
                {
                    var memos = _tpsFile.GetTpsMemos(
                        table: _tableNumber,
                        owningRecord: _dataRecordPayloadEnumerator.Current.RecordNumber,
                        memoDefinitionIndex: memoIndex);

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

            throw new IndexOutOfRangeException($"No such column name: '{name}'.");
        }
    }

    public override int Depth
    {
        get
        {
            ThrowIfDisposed();

            return 0;
        }
    }

    public override bool IsClosed => !IsDisposed;

    public override int RecordsAffected => 0;

    public override int FieldCount
    {
        get
        {
            ThrowIfDisposed();

            return _columnDefinitions.Count + VIRTUAL_FIELDS_TOTAL;
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
        if (ordinal == _columnDefinitions.Count + VIRTUAL_FIELDS_RECORDNUMBER_OFFSET)
        {
            //This will be VIRTUAL_FIELDS_RECORDNUMBER
            return typeof(long);
        }
        
        var (FieldDef, MemoDef) = _columnDefinitions[ordinal];

        if (FieldDef is FieldDefinition { } fieldDef)
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
                _ => throw new NotImplementedException(),
            };

        }

        if (MemoDef is MemoDefinition { } memoDef)
        {
            if (memoDef.IsBlob)
            {
                return typeof(byte[]);
            }
            if (memoDef.IsTextMemo)
            {
                return typeof(string);
            }
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
        ThrowIfDisposed();

        var ret = IndexToColumnNames[ordinal];
        return ret;
    }

    public override int GetOrdinal(string name)
    {
        ThrowIfDisposed();

        var ret = ColumnNamesToIndex[name];
        return ret;
    }

    public override DataTable? GetSchemaTable()
    {
        throw new NotImplementedException();
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
        ThrowIfDisposed();

        // TODO should return true if a BLOB or MEMO is not present.

        return false;
    }

    public override bool NextResult()
    {
        return false;
    }

    public override bool Read()
    {
        _dataRecordPayloadEnumerator ??= _dataRecordPayloads.GetEnumerator();

        if (!_dataRecordPayloadEnumerator.MoveNext())
        {
            return false;
        }

        _currentEnumerationResults = FieldValueReader.EnumerateValues(_fieldIteratorNodes, _dataRecordPayloadEnumerator.Current);

        return true;
    }

    public override IEnumerator GetEnumerator()
    {
        throw new NotImplementedException();
    }
}
