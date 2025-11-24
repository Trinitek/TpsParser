using System;
using System.Collections.Generic;
using System.Linq;

namespace TpsParser.Data;

/// <summary>
/// Speeds up lookup times for <c>MEMO</c> and <c>BLOB</c> records.
/// </summary>
public sealed class MemoIndexer
{
    private readonly record struct TableNumberKey(
        int TableNumber);

    private readonly record struct MemoIndexKey(
        int RecordNumber,
        byte DefinitionIndex);

    private readonly TpsFile _file;

    // Benchmarking on .NET 9 x64 with a 17,000-record file, single MEMO definition, indicates a
    // FrozenDictionary<MemoIndexKey, List<MemoRecordPayload>> is slightly slower by a few milliseconds
    // both in construction and enumeration. No clear benefit over a plain Dictionary<,>.
    private Dictionary<TableNumberKey, Dictionary<MemoIndexKey, List<MemoRecordPayload>>>? _memoPayloadLookup;

    /// <summary>
    /// Instantiates a new indexer for a file.
    /// </summary>
    /// <param name="file"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public MemoIndexer(TpsFile file)
    {
        _file = file ?? throw new ArgumentNullException(nameof(file));
    }

    /// <summary>
    /// Ensures <c>MEMO</c> and <c>BLOB</c> payloads are indexed for the given table numbers.
    /// </summary>
    /// <param name="requestedTableNumbers"></param>
    /// <exception cref="ArgumentException"></exception>
    public void EnsureBuiltForTables(params int[] requestedTableNumbers)
    {
        ArgumentNullException.ThrowIfNull(requestedTableNumbers);

        if (requestedTableNumbers.Length == 0)
        {
            throw new ArgumentException("At least one table number should be specified.", nameof(requestedTableNumbers));
        }

        var tableDefs = _file.GetTableDefinitions();

        foreach (var tableNumber in requestedTableNumbers)
        {
            if (tableDefs.Keys.Contains(tableNumber) is false)
            {
                throw new ArgumentException($"Table number does not exist in file: {tableNumber}.");
            }
        }

        var existingTableLookup = _memoPayloadLookup ??= [];

        int[] tableNumbersToBuild = [.. requestedTableNumbers.Except(existingTableLookup.Keys.Select(k => k.TableNumber))];

        if (tableNumbersToBuild.Length == 0)
        {
            // All requested table indexes are already built; nothing to do.
            return;
        }

        Dictionary<TableNumberKey, Dictionary<MemoIndexKey, List<MemoRecordPayload>>> builder = [];

        foreach (var tableNumber in tableNumbersToBuild)
        {
            builder.Add(new(tableNumber), []);
        }

        var memoPayloads = _file.EnumerateMemoRecordPayloads();

        foreach (var memoPayload in memoPayloads)
        {
            if (!requestedTableNumbers.Contains(memoPayload.TableNumber))
            {
                continue;
            }

            TableNumberKey tableNumberKey = new(
                TableNumber: memoPayload.TableNumber);

            MemoIndexKey memoIndexKey = new(
                RecordNumber: memoPayload.RecordNumber,
                DefinitionIndex: memoPayload.DefinitionIndex);

            var memoDictForTable = builder[tableNumberKey];

            if (!memoDictForTable.TryGetValue(memoIndexKey, out var payloads))
            {
                payloads = [];
                memoDictForTable.Add(memoIndexKey, payloads);
            }

            payloads.Add(memoPayload);
        }

        foreach (var tableLookup in builder)
        {
            _memoPayloadLookup.Add(tableLookup.Key, tableLookup.Value);
        }
    }

    /// <summary>
    /// Constructs a complete <c>MEMO</c> or <c>BLOB</c> from the previously indexed payloads,
    /// or returns <see langword="null"/> if one does not exist or has incomplete data.
    /// </summary>
    /// <param name="tableDefinition"></param>
    /// <param name="tableNumber"></param>
    /// <param name="owningRecord"></param>
    /// <param name="definitionIndex"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public ITpsMemo? GetValue(
        TableDefinition tableDefinition,
        int tableNumber,
        int owningRecord,
        byte definitionIndex)
    {
        if (_memoPayloadLookup is null
            || !_memoPayloadLookup.TryGetValue(new(tableNumber), out var memoIndexes))
        {
            throw new InvalidOperationException($"Memo index has not been built yet for table number {tableNumber}.");
        }

        if (!memoIndexes.TryGetValue(new(RecordNumber: owningRecord, DefinitionIndex: definitionIndex), out var payloads))
        {
            return null;
        }

        if (payloads.Count == 0)
        {
            return null;
        }

        var constructedMemos = TpsMemoBuilder.BuildTpsMemos(payloads, tableDefinition);

        return constructedMemos.FirstOrDefault();
    }
}
