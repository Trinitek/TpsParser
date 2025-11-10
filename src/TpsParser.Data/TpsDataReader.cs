using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TpsParser.TypeModel;

namespace TpsParser.Data;

public class TpsDataReader : System.Data.Common.DbDataReader
{
    private TpsParser? Parser;
    private List<(int, TableDefinition)> TableDefinitions;

    private int? TableDefinitionIndex;

    private int? TableDefinitionId;
    private TableDefinition? TableDefinition;

    private Dictionary<int, string> IndexToColumnNames = new();
    private Dictionary<string, int> ColumnNamesToIndex = new();
    private HashSet<string> ColumnNames_WithCase = new();
    private Dictionary<string, string> ColumnNames_ToCase = new();
    private List<object> ColumnDefinitions = new();

    private List<Dictionary<string, object>> Rows { get; set; } = new();
    private int RowIndex;

    private const int VIRTUAL_FIELDS_TOTAL = 1;
    private const string VIRTUAL_FIELDS_RECORDNUMBER = "__RECORD_NUMBER";
    private const int VIRTUAL_FIELDS_RECORDNUMBER_OFFSET = 0;

    private bool IsDisposed;
    public override void Close() {
        base.Close();
        
        Parser?.Dispose();
        Parser = null;
        TableDefinitions.Clear();
        IndexToColumnNames.Clear();
        ColumnNamesToIndex.Clear();
        ColumnNames_WithCase.Clear();
        ColumnNames_ToCase.Clear();
        ColumnDefinitions.Clear();
        Rows.Clear();
        IsDisposed = true;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
    }

    public TpsDataReader(TpsParser Parser, IReadOnlyDictionary<int, TableDefinition> TableDefinitions)
    {
        this.Parser = Parser;
        this.TableDefinitions = TableDefinitions.Select(x => (x.Key, x.Value)).ToList();
    }

    public override bool HasRows
    {
        get
        {
            ThrowIfDisposed();

            return Rows.Any();
        }
    }

    public override object this[int i]
    {
        get
        {
            ThrowIfDisposed();

            var ColumnName = GetName(i);
            var ret = this[ColumnName];

            return ret;
        }
    }

    public override object this[string name]
    {
        get
        {
            ThrowIfDisposed();

            var Row = Rows[RowIndex];
            var Name = GetName(name);

            var ret = Row[Name];
            return ret;
        }
    }

    public override int Depth
    {
        get
        {
            ThrowIfDisposed();

            return TableDefinitionIndex ?? 0;
        }
    }

    public override bool IsClosed => !IsDisposed;

    public override int RecordsAffected => 0;

    public override int FieldCount
    {
        get
        {
            ThrowIfDisposed();

            return ColumnDefinitions.Count + VIRTUAL_FIELDS_TOTAL;
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

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
    public override Type GetFieldType(int ordinal)
    {
        var ret = default(Type?);

        if (ordinal == ColumnDefinitions.Count + VIRTUAL_FIELDS_RECORDNUMBER_OFFSET)
        {
            //This will be VIRTUAL_FIELDS_RECORDNUMBER
            ret = typeof(long);
        }
        else
        {

            var V = ColumnDefinitions[ordinal];

            if (V is FieldDefinition { } V1)
            {
                ret = V1.TypeCode switch
                {
                    ClaTypeCode.Byte => typeof(byte),
                    ClaTypeCode.Short => typeof(short),
                    ClaTypeCode.UShort => typeof(ushort),
                    ClaTypeCode.Date => typeof(DateOnly),
                    ClaTypeCode.Time => typeof(TimeOnly),
                    ClaTypeCode.Long => typeof(long),
                    ClaTypeCode.ULong => typeof(ulong),
                    ClaTypeCode.SReal => typeof(double),
                    ClaTypeCode.Real => typeof(double),
                    ClaTypeCode.Decimal => typeof(decimal),
                    ClaTypeCode.FString => typeof(string),
                    ClaTypeCode.CString => typeof(string),
                    ClaTypeCode.PString => typeof(string),
                    ClaTypeCode.Group => typeof(object),
                    //ClaTypeCode.Blob => typeof(byte[]),
                    _ => throw new NotImplementedException(),
                };

            }
            else if (V is MemoDefinition { } V2)
            {
                if (V2.IsBlob)
                {
                    ret = typeof(byte[]);
                }
                if (V2.IsMemo)
                {
                    ret = typeof(string);
                }
            }
        }

        if(ret is null)
        {
            throw new NotImplementedException();
        }

        return ret;
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

    public string GetName(string Name)
    {
        ThrowIfDisposed();

        var ret = Name;

        if (!ColumnNames_WithCase.Contains(Name) && ColumnNames_ToCase.TryGetValue(Name, out var tret))
        {
            ret = tret;
        }

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

        return false;
    }

    public override bool NextResult()
    {
        var ret = false;
        var Next = (TableDefinitionIndex ?? -1) + 1;

        if (Next < TableDefinitions.Count)
        {
            ret = true;

            TableDefinitionIndex = Next;
            var (ActualTableDefinitionId, ActualTableDefinition) = TableDefinitions[Next];
            (TableDefinitionId, TableDefinition) = (ActualTableDefinitionId, ActualTableDefinition);

            {
                var NewIndexToColumnNames = new Dictionary<int, string>();
                foreach (var Field in TableDefinition.Fields)
                {
                    NewIndexToColumnNames[NewIndexToColumnNames.Count] = Field.Name;
                    ColumnDefinitions.Add(Field);
                }

                foreach (var Field in TableDefinition.Memos)
                {
                    NewIndexToColumnNames[NewIndexToColumnNames.Count] = Field.Name;
                    ColumnDefinitions.Add(Field);
                }

                //Create a dummy member
                NewIndexToColumnNames[NewIndexToColumnNames.Count] = VIRTUAL_FIELDS_RECORDNUMBER;

                IndexToColumnNames = NewIndexToColumnNames;
                ColumnNamesToIndex = NewIndexToColumnNames.DistinctBy(x => x.Value).ToDictionary(x => x.Value, x => x.Key);

                ColumnNames_WithCase = NewIndexToColumnNames.Select(x => x.Value).ToHashSet();
                ColumnNames_ToCase = NewIndexToColumnNames.Select(x => x.Value).Distinct(StringComparer.InvariantCulture).ToDictionary(x => x, x => x, StringComparer.InvariantCultureIgnoreCase);
            }

            {
                var dataRecords = GatherDataRecords(ActualTableDefinitionId, ActualTableDefinition, true);
                var memoRecords = GatherMemoRecords(ActualTableDefinitionId, ActualTableDefinition, true);

                var unifiedRecords = new Dictionary<int, Dictionary<string, object>>();

                foreach (var dataKvp in dataRecords)
                {
                    var Values = dataKvp.Value.ToDictionary(pair => pair.Key, pair => pair.Value.Value);

                    foreach (var Record in ActualTableDefinition.Memos)
                    {
                        if (Record.IsMemo)
                        {
                            Values[Record.Name] = string.Empty;
                        }
                        if (Record.IsBlob)
                        {
                            Values[Record.Name] = Array.Empty<byte>();
                        }
                    }


                    unifiedRecords.Add(dataKvp.Key, Values);
                }

                foreach (var memoRecord in memoRecords)
                {
                    var recordNumber = memoRecord.Key;

                    var dataNameValues = dataRecords[recordNumber];

                    foreach (var memoNameValue in memoRecord.Value)
                    {
                        unifiedRecords[recordNumber][memoNameValue.Key] = memoNameValue.Value.Value;
                    }
                }

                foreach (var (RecordNumber, RecordSet) in unifiedRecords)
                {
                    unifiedRecords[RecordNumber][VIRTUAL_FIELDS_RECORDNUMBER] = RecordNumber;
                }

                Rows = unifiedRecords.Select(x => x.Value).ToList();
                RowIndex = -1;
                unifiedRecords.Equals(unifiedRecords);
            }
        }

        return ret;
    }

    private IReadOnlyDictionary<int, IReadOnlyDictionary<string, IClaObject>> GatherDataRecords(int table, TableDefinition tableDefinitionRecord, bool ignoreErrors)
    {
        var dataRecords = Parser?.TpsFile.GetDataRows(table, tableDefinitionRecord: tableDefinitionRecord, ignoreErrors);

        return dataRecords.EmptyIfNull().ToDictionary(r => r.RecordNumber, r => r.GetFieldValuePairs());
    }

    private IReadOnlyDictionary<int, IReadOnlyDictionary<string, IClaObject>> GatherMemoRecords(int table, TableDefinition tableDefinitionRecord, bool ignoreErrors)
    {
        var ret = Enumerable.Range(0, tableDefinitionRecord.Memos.Length)
            .SelectMany(index => {
                var definition = tableDefinitionRecord.Memos[index];
                var memoRecordsForIndex = Parser?.TpsFile.GetMemoRecords(table, index, ignoreErrors);

                return memoRecordsForIndex.EmptyIfNull().Select(record => (owner: record.RecordNumber, name: definition.Name, value: record.GetValue(definition)));
            })
            .GroupBy(pair => pair.owner, pair => (pair.name, pair.value))
            .ToDictionary(
                groupedPair => groupedPair.Key,
                groupedPair => (IReadOnlyDictionary<string, IClaObject>)groupedPair
                    .ToDictionary(pair => pair.name, pair => pair.value));

        return ret;
    }

    public override bool Read()
    {
        var ret = false;
        var NextIndex = RowIndex + 1;

        if (NextIndex < Rows.Count) {
            RowIndex = NextIndex;
            ret = true;
        }

        return ret;
    }

    public override IEnumerator GetEnumerator()
    {
        throw new NotImplementedException();
    }
}
