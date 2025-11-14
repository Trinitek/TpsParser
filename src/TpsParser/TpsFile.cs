using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TpsParser.Tps.Record;

namespace TpsParser;

/// <summary>
/// Represents a TopSpeed file and provides access to low level file and record structures.
/// </summary>
public sealed class TpsFile
{
    private TpsRandomAccess Data { get; }

    public EncodingOptions EncodingOptions { get; }
    public ErrorHandlingOptions ErrorHandlingOptions { get; }

    public TpsFile(
        Stream stream,
        EncodingOptions? encodingOptions = null,
        ErrorHandlingOptions? errorHandlingOptions = null)
    {
        ArgumentNullException.ThrowIfNull(stream);
        EncodingOptions = encodingOptions ?? EncodingOptions.Default;
        ErrorHandlingOptions = errorHandlingOptions ?? ErrorHandlingOptions.Default;

        byte[] fileData;

        using (var ms = new MemoryStream())
        {
            stream.CopyTo(ms);
            fileData = ms.ToArray();
        }

        Data = new TpsRandomAccess(fileData, EncodingOptions.ContentEncoding);
    }

    public TpsFile(
        Stream stream,
        Key key,
        EncodingOptions? encodingOptions = null,
        ErrorHandlingOptions? errorHandlingOptions = null)
        : this(
              stream,
              encodingOptions,
              errorHandlingOptions)
    {
        ArgumentNullException.ThrowIfNull(key);

        Decrypt(key);
    }

    private void Decrypt(Key key)
    {
        key.Decrypt(new TpsRandomAccess(Data, 0, 0x200));

        var header = GetFileHeader();

        foreach (var pageRange in header.BlockDescriptors)
        {
            uint offset = pageRange.StartOffset;
            uint end = pageRange.EndOffset;

            if ((offset != 0x200 || end != 0x200) && offset < Data.Length)
            {
                key.Decrypt(new TpsRandomAccess(Data, (int)offset, (int)(end - offset)));
            }
        }
    }

    /// <summary>
    /// Gets the file header.
    /// </summary>
    /// <returns></returns>
    public TpsFileHeader GetFileHeader()
    {
        Data.JumpAbsolute(0);

        var header = TpsFileHeader.Parse(Data);

        if (!header.IsTopSpeedFile)
        {
            throw new TpsParserException($"Not a TopSpeed file ({header.MagicNumber})");
        }

        return header;
    }

    /// <summary>
    /// Gets all the blocks in the file.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TpsBlock> GetBlocks()
    {
        var header = GetFileHeader();

        var blocks = header.BlockDescriptors
            // Skip zero-length pages and any blocks that are beyond the file size.
            .Where(range => !(range.Length == 0 || range.StartOffset >= Data.Length))

            .Select(range => TpsBlock.Parse(range, Data));

        return blocks;
    }

    public IEnumerable<TpsRecord> EnumerateRecords(ErrorHandlingOptions? errorHandlingOptions = null)
    {
        errorHandlingOptions ??= ErrorHandlingOptions;

        foreach (var block in GetBlocks())
        {
            foreach (var page in block.GetPages())
            {
                foreach (var record in page.GetRecords(errorHandlingOptions))
                {
                    yield return record;
                }
            }
        }
    }

    /// <summary>
    /// Gets a list of data records for the associated table and its table definition.
    /// </summary>
    /// <param name="table">The table from which to get the records.</param>
    /// <param name="tableDefinition">The table definition that describes the table schema.</param>
    /// <param name="errorHandlingOptions"></param>
    /// <returns></returns>
    public IEnumerable<IDataRecord> GetDataRows(int table, TableDefinition tableDefinition, ErrorHandlingOptions? errorHandlingOptions = null)
    {
        return EnumerateRecords(errorHandlingOptions)
            .Where(record => record.GetPayload() is DataRecordPayload pl && pl.TableNumber == table)
            .Select(record => new DataRecord(record, tableDefinition, EncodingOptions.ContentEncoding));
    }

    /// <summary>
    /// Gets a list of data records for the associated table and its table definition.
    /// </summary>
    /// <param name="table">The table from which to get the records.</param>
    /// <param name="tableDefinition">The table definition that describes the table schema.</param>
    /// <param name="errorHandlingOptions"></param>
    /// <returns></returns>
    public IEnumerable<DataRecordPayload> GetDataRecordPayloads(
        int table,
        TableDefinition tableDefinition,
        ErrorHandlingOptions? errorHandlingOptions = null)
    {
        return GetDataRecordPayloads(
            table: table,
            errorHandlingOptions: errorHandlingOptions);
    }

    public IEnumerable<DataRecordPayload> GetDataRecordPayloads(
        int table,
        ErrorHandlingOptions? errorHandlingOptions = null)
    {
        IEnumerable<DataRecordPayload> VisitData()
        {
            var records = EnumerateRecords(errorHandlingOptions);

            foreach (var r in records)
            {
                if (r.PayloadType != RecordPayloadType.Data)
                {
                    continue;
                }

                var payload = new DataRecordPayload { PayloadData = r.PayloadData };

                if (payload.TableNumber != table)
                {
                    continue;
                }

                yield return payload;
            }
        }

        return VisitData();
    }

    /// <summary>
    /// Gets a list of table name records that describe the name of the tables included in the file.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TableNameRecordPayload> GetTableNameRecordPayloads()
    {
        return EnumerateRecords()
            .Where(record => record.GetPayload() is TableNameRecordPayload)
            .Select(record => (TableNameRecordPayload)record.GetPayload()!);
    }

    public IEnumerable<IndexRecordPayload> GetIndexRecordPayloads(int table, int index)
    {
        return EnumerateRecords()
            .Where(record =>
                record.GetPayload() is IndexRecordPayload payload
                && payload.TableNumber == table
                && (payload.DefinitionIndex == index || index == -1))
            .Select(record => (IndexRecordPayload)record.GetPayload()!);
    }

    /// <summary>
    /// Gets a list of metadata that is included for the associated table.
    /// </summary>
    /// <param name="table">The table for which to get the metadata.</param>
    /// <returns></returns>
    public IEnumerable<MetadataRecordPayload> GetMetadataRecordPayloads(int table)
    {
        return EnumerateRecords()
            .Where(record => record.GetPayload() is MetadataRecordPayload header && header.TableNumber == table)
            .Select(r => (MetadataRecordPayload)r.GetPayload()!);
    }

    /// <summary>
    /// Gets all of the records in the file.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TpsRecord> GetTpsRecords()
    {
        return EnumerateRecords();
    }

    /// <summary>
    /// Gets a dictionary of memo and blob records for the associated table.
    /// </summary>
    /// <param name="table">The table number that owns the memos.</param>
    /// <param name="owningRecord">The record number that owns the memo.</param>
    /// <param name="memoIndex">The index number of the memo in the record, zero-based. Records can have more than one memo.</param>
    /// <param name="errorHandlingOptions"></param>
    /// <returns></returns>
    public IEnumerable<ITpsMemo> GetTpsMemos(
        int table,
        int? owningRecord = null,
        byte? memoIndex = null,
        ErrorHandlingOptions? errorHandlingOptions = null)
    {
        var memoRecords = EnumerateMemoRecordPayloads(
            table: table,
            owningRecord: owningRecord,
            memoDefinitionIndex: memoIndex,
            errorHandlingOptions: errorHandlingOptions);

        var tableDef = GetTableDefinitions()[table];

        return TpsMemoBuilder.BuildTpsMemos(memoRecords, tableDef);
    }

    public IEnumerable<MemoRecordPayload> EnumerateMemoRecordPayloads(
        int table,
        int? owningRecord = null,
        byte? memoDefinitionIndex = null,
        ErrorHandlingOptions? errorHandlingOptions = null)
    {
        IEnumerable<MemoRecordPayload> VisitMemos()
        {
            var records = EnumerateRecords(errorHandlingOptions);

            foreach (var r in records)
            {
                if (r.PayloadType != RecordPayloadType.Memo)
                {
                    continue;
                }

                var payload = new MemoRecordPayload { PayloadData = r.PayloadData };

                if (payload.TableNumber != table)
                {
                    continue;
                }

                if (owningRecord.HasValue && payload.RecordNumber != owningRecord.Value)
                {
                    continue;
                }

                if (memoDefinitionIndex.HasValue && payload.DefinitionIndex != memoDefinitionIndex.Value)
                {
                    continue;
                }

                yield return payload;
            }
        }

        return VisitMemos();
    }

    /// <summary>
    /// Gets a dictionary of table definitions and their associated table numbers.
    /// </summary>
    /// <param name="errorHandlingOptions"></param>
    /// <returns></returns>
    public IReadOnlyDictionary<int, TableDefinition> GetTableDefinitions(ErrorHandlingOptions? errorHandlingOptions = null)
    {
        return EnumerateRecords(errorHandlingOptions)
            .Where(record => record.GetPayload() is TableDefinitionRecordPayload)
            .Select(record => (TableDefinitionRecordPayload)record.GetPayload()!)

            // Records must be merged in order according to the header's sequence number.
            .OrderBy(record => record.SequenceNumber)

            // Group records by table number.
            .GroupBy(record => record.TableNumber)

            // Do not process groups that have skipped block indexes. (i.e. 0, 1, 3, 4)
            .Where(group => group.Count() == group.Last().SequenceNumber + 1)

            .ToDictionary(
            keySelector: group => group.Key,
            elementSelector: group =>
                //TableDefinition.Parse(Merge(group))
                TableDefinition.Parse(
                    new TpsRandomAccess(
                        MergeMemory(
                            group.Select(r => r.Content)).ToArray(),
                        EncodingOptions.MetadataEncoding))
            );
    }

    private ReadOnlyMemory<byte> MergeMemory(IEnumerable<ReadOnlyMemory<byte>> memories)
    {
        var mm = memories.ToList();

        byte[] buffer = new byte[mm.Sum(m => m.Length)];
        var bufferMem = buffer.AsMemory();
        
        int bufferOfs = 0;

        foreach (var m in mm)
        {
            m.CopyTo(bufferMem[bufferOfs..]);

            bufferOfs += m.Length;
        }

        return buffer;
    }
}
