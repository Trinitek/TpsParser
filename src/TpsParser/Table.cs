using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TpsParser.TypeModel;

namespace TpsParser;

/// <summary>
/// Provides a high level representation of a table within a TopSpeed file.
/// </summary>
public sealed class Table
{
    /// <summary>
    /// Gets the name of the table.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets an unsorted collection of rows that belong to this table.
    /// </summary>
    public IEnumerable<Row> Rows { get; }

    /// <summary>
    /// Instantiates a new table.
    /// </summary>
    /// <param name="name">The name of the table.</param>
    /// <param name="rows">The rows that belong to the table.</param>
    public Table(string name, IEnumerable<Row> rows)
    {
        Name = name;
        Rows = rows ?? throw new ArgumentNullException(nameof(rows));
    }

    private static IReadOnlyDictionary<int, IReadOnlyDictionary<string, IClaObject>> GatherDataRecords(TpsFile file, int table, TableDefinition tableDefinitionRecord, ErrorHandlingOptions? errorHandlingOptions)
    {
        var nodes = FieldValueReader.CreateFieldIteratorNodes(
            fieldDefinitions: tableDefinitionRecord.Fields,
            requestedFieldIndexes: [.. tableDefinitionRecord.Fields.Select(f => f.Index)]);

        var dataRecordPayloads = file.GetDataRecordPayloads(table, errorHandlingOptions);

        return dataRecordPayloads.ToDictionary(
            keySelector: r => r.RecordNumber,
            elementSelector: r =>
            {
                return (IReadOnlyDictionary<string, IClaObject>)FieldValueReader.EnumerateValues(nodes, r)
                    .ToDictionary(
                        keySelector: fieldEnumResult => fieldEnumResult.FieldDefinition.Inner.Name,
                        elementSelector: fieldEnumResult => fieldEnumResult.Value)
                    .AsReadOnly();
            }).AsReadOnly();
    }

    private static IReadOnlyDictionary<int, IReadOnlyDictionary<string, ITpsMemo>> GatherMemoRecords(TpsFile file, int table, TableDefinition tableDefinitionRecord, ErrorHandlingOptions? errorHandlingOptions)
    {
        return tableDefinitionRecord.Memos
            .SelectMany((definition, index) =>
            {
                var tpsMemosForIndex = file.GetTpsMemos(table, memoDefinitionIndex: (byte)index, errorHandlingOptions: errorHandlingOptions);

                return tpsMemosForIndex.Select(record =>
                    (owner: record.RecordNumber, name: definition.Name, value: record)
                );
            })
            .GroupBy(pair => pair.owner, pair => (pair.name, pair.value))
            .ToDictionary(
                keySelector: groupedPair => groupedPair.Key,
                elementSelector: groupedPair => (IReadOnlyDictionary<string, ITpsMemo>)groupedPair
                    .ToDictionary(pair => pair.name, pair => pair.value));
    }

    /// <summary>
    /// Constructs a high-level representation of a table in the file.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="tableNumber">The table number, or <see langword="null"/> to get the first table in the file.</param>
    /// <param name="encodingOptions"></param>
    /// <param name="errorHandlingOptions"></param>
    /// <returns></returns>
    public static Table MaterializeFromFile(
        TpsFile file,
        int? tableNumber = null,
        EncodingOptions? encodingOptions = null,
        ErrorHandlingOptions? errorHandlingOptions = null)
    {
        encodingOptions ??= file.EncodingOptions;

        var tableNameDefinitions = file.GetTableNameRecordPayloads();

        var tableDefinitions = file.GetTableDefinitions(errorHandlingOptions);

        var firstTableDefinition = tableNumber.HasValue
            ? new KeyValuePair<int, TableDefinition>(tableNumber.Value, tableDefinitions[tableNumber.Value])
            : tableDefinitions.First();

        var dataRecords = GatherDataRecords(file, firstTableDefinition.Key, firstTableDefinition.Value, errorHandlingOptions);
        var memoRecords = GatherMemoRecords(file, firstTableDefinition.Key, firstTableDefinition.Value, errorHandlingOptions);

        var rows = dataRecords.Select(dataKvp =>
        {
            var recordNumber = dataKvp.Key;

            IReadOnlyDictionary<string, ITpsMemo> memoValues;

            if (memoRecords.TryGetValue(recordNumber, out var memosForRecord))
            {
                memoValues = memosForRecord;
            }
            else
            {
                memoValues = ReadOnlyDictionary<string, ITpsMemo>.Empty;
            }

            return new Row(recordNumber, dataKvp.Value, memoValues);
        });

        string tableName = tableNameDefinitions
            .First(n => n.TableNumber == firstTableDefinition.Key).GetName(encodingOptions.MetadataEncoding);

        var table = new Table(tableName, rows);

        return table;
    }
}
