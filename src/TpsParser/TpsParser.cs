using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TpsParser.Tps;
using TpsParser.Tps.Record;
using TpsParser.TypeModel;

namespace TpsParser;

/// <summary>
/// An easy to use reader and object deserializer for TopSpeed files.
/// </summary>
public sealed class TpsParser : IDisposable
{
    /// <summary>
    /// Gets the low level representation of the TopSpeed file and its data structures.
    /// </summary>
    public TpsFile TpsFile { get; }

    private Stream Stream { get; }

    internal TpsParser(TpsFile tpsFile)
    {
        TpsFile = tpsFile ?? throw new ArgumentNullException(nameof(tpsFile));
    }

    /// <summary>
    /// Instantiates a new parser.
    /// </summary>
    /// <param name="stream">The stream from which to read the TopSpeed file.</param>
    public TpsParser(Stream stream)
    {
        Stream = stream ?? throw new ArgumentNullException(nameof(stream));
        TpsFile = new RandomAccessTpsFile(Stream);
    }

    /// <summary>
    /// Instantiates a new parser.
    /// </summary>
    /// <param name="stream">The stream from which to read the TopSpeed file.</param>
    /// <param name="password">The password or "owner" to use to decrypt the file.</param>
    public TpsParser(Stream stream, string password)
    {
        Stream = stream ?? throw new ArgumentNullException(nameof(stream));
        TpsFile = new RandomAccessTpsFile(Stream, new Key(password));
    }

    /// <summary>
    /// Instantiates a new parser.
    /// </summary>
    /// <param name="filename">The filename of the TopSpeed file.</param>
    public TpsParser(string filename)
    {
        Stream = new FileStream(filename, FileMode.Open);
        TpsFile = new RandomAccessTpsFile(Stream);
    }

    /// <summary>
    /// Instantiates a new parser.
    /// </summary>
    /// <param name="filename">The filename of the TopSpeed file.</param>
    /// <param name="password">The password or "owner" to use to decrypt the file.</param>
    public TpsParser(string filename, string password)
    {
        Stream = new FileStream(filename, FileMode.Open);
        TpsFile = new RandomAccessTpsFile(Stream, new Key(password));
    }

    private IReadOnlyDictionary<int, IReadOnlyDictionary<string, IClaObject>> GatherDataRecords(int table, TableDefinition tableDefinitionRecord, bool ignoreErrors)
    {
        var dataRecords = TpsFile.GetDataRecords(table, tableDefinitionRecord: tableDefinitionRecord, ignoreErrors);

        return dataRecords.ToDictionary(r => r.RecordNumber, r => r.GetFieldValuePairs());
    }

    private IReadOnlyDictionary<int, IReadOnlyDictionary<string, IClaObject>> GatherMemoRecords(int table, TableDefinition tableDefinitionRecord, bool ignoreErrors)
    {
        return Enumerable.Range(0, tableDefinitionRecord.Memos.Count())
            .SelectMany(index =>
            {
                var definition = tableDefinitionRecord.Memos[index];
                var memoRecordsForIndex = TpsFile.GetMemoRecords(table, index, ignoreErrors);

                return memoRecordsForIndex.Select(record => (owner: record.Header.OwningRecord, name: definition.Name, value: record.GetValue(definition)));
            })
            .GroupBy(pair => pair.owner, pair => (pair.name, pair.value))
            .ToDictionary(
                groupedPair => groupedPair.Key,
                groupedPair => (IReadOnlyDictionary<string, IClaObject>)groupedPair
                    .ToDictionary(pair => pair.name, pair => pair.value));
    }

    /// <summary>
    /// Gets a high level representation of the first table in the file.
    /// </summary>
    /// <param name="ignoreErrors">If true, the reader will not throw an exception when it encounters unexpected data.</param>
    /// <returns></returns>
    public Table BuildTable(bool ignoreErrors = false)
    {
        var tableNameDefinitions = TpsFile.GetTableNameRecords();

        var tableDefinitions = TpsFile.GetTableDefinitions(ignoreErrors: ignoreErrors);

        var firstTableDefinition = tableDefinitions.First();

        var dataRecords = GatherDataRecords(firstTableDefinition.Key, firstTableDefinition.Value, ignoreErrors);
        var memoRecords = GatherMemoRecords(firstTableDefinition.Key, firstTableDefinition.Value, ignoreErrors);

        var unifiedRecords = new Dictionary<int, Dictionary<string, IClaObject>>();

        foreach (var dataKvp in dataRecords)
        {
            unifiedRecords.Add(dataKvp.Key, dataKvp.Value.ToDictionary(pair => pair.Key, pair => pair.Value));
        }

        foreach (var memoRecord in memoRecords)
        {
            int recordNumber = memoRecord.Key;

            var dataNameValues = dataRecords[recordNumber];

            foreach (var memoNameValue in memoRecord.Value)
            {
                unifiedRecords[recordNumber].Add(memoNameValue.Key, memoNameValue.Value);
            }
        }

        var rows = unifiedRecords.Select(r => new Row(r.Key, r.Value));

        string tableName = tableNameDefinitions
            .First(n => n.TableNumber == firstTableDefinition.Key).Header.Name;

        var table = new Table(tableName, rows);

        return table;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Stream?.Dispose();
    }
}
