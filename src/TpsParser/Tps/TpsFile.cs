using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TpsParser.Tps.Record;

namespace TpsParser.Tps;

/// <summary>
/// Represents a TopSpeed file and provides access to low level file and record structures.
/// </summary>
public abstract class TpsFile
{
    /// <summary>
    /// Gets or sets the encoding to use when reading strings in the TPS file.
    /// The default is ISO-8859-1.
    /// </summary>
    public Encoding Encoding
    {
        get => _encoding;
        set => _encoding = value ?? throw new ArgumentNullException(nameof(value));
    }

    private Encoding _encoding = Encoding.GetEncoding("ISO-8859-1");

    /// <summary>
    /// Gets the file header.
    /// </summary>
    /// <returns></returns>
    public abstract TpsFileHeader GetFileHeader();

    public abstract IEnumerable<TpsBlock> GetBlocks();

    /// <summary>
    /// Gets a list of data records for the associated table and its table definition.
    /// </summary>
    /// <param name="table">The table from which to get the records.</param>
    /// <param name="tableDefinitionRecord">The table definition that describes the table schema.</param>
    /// <param name="ignoreErrors">True if exceptions should not be thrown when unexpected data is encountered.</param>
    /// <returns></returns>
    public abstract IEnumerable<IDataRecord> GetDataRows(int table, TableDefinition tableDefinitionRecord, bool ignoreErrors);

    /// <summary>
    /// Gets a list of data records for the associated table and its table definition.
    /// </summary>
    /// <param name="table">The table from which to get the records.</param>
    /// <param name="tableDefinitionRecord">The table definition that describes the table schema.</param>
    /// <param name="ignoreErrors">True if exceptions should not be thrown when unexpected data is encountered.</param>
    /// <returns></returns>
    public abstract IEnumerable<DataRecordPayload> GetDataRecords(int table, TableDefinition tableDefinitionRecord, bool ignoreErrors);

    /// <summary>
    /// Gets a list of table name records that describe the name of the tables included in the file.
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerable<TableNameRecordPayload> GetTableNameRecords();

    public abstract IEnumerable<IndexRecordPayload> GetIndexes(int table, int index);

    /// <summary>
    /// Gets a list of metadata that is included for the associated table.
    /// </summary>
    /// <param name="table">The table for which to get the metadata.</param>
    /// <returns></returns>
    public abstract IEnumerable<MetadataRecordPayload> GetMetadata(int table);

    /// <summary>
    /// Gets all of the records in the file.
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerable<TpsRecord> GetTpsRecords();

    /// <summary>
    /// Gets a dictionary of memo and blob records for the associated table.
    /// </summary>
    /// <param name="table">The table number that owns the memos.</param>
    /// <param name="ignoreErrors">True if exceptions should not be thrown when unexpected data is encountered.</param>
    /// <returns></returns>
    public abstract IEnumerable<MemoRecordPayload> GetMemoRecords(int table, bool ignoreErrors);

    /// <summary>
    /// Gets a dictionary of memo and blob records for the associated table.
    /// </summary>
    /// <param name="table">The table number that owns the memo.</param>
    /// <param name="memoIndex">The index number of the memo in the record, zero-based. Records can have more than one memo.</param>
    /// <param name="ignoreErrors">True if exceptions should not be thrown when unexpected data is encountered.</param>
    /// <returns></returns>
    public abstract IEnumerable<MemoRecordPayload> GetMemoRecords(int table, int memoIndex, bool ignoreErrors);

    /// <summary>
    /// Gets a dictionary of table definitions and their associated table numbers.
    /// </summary>
    /// <param name="ignoreErrors">True if exceptions should not be thrown when unexpected data is encountered.</param>
    /// <returns></returns>
    public abstract IReadOnlyDictionary<int, TableDefinition> GetTableDefinitions(bool ignoreErrors);
}

/// <inheritdoc/>
internal sealed class RandomAccessTpsFile : TpsFile
{
    private TpsRandomAccess Data { get; }

    public RandomAccessTpsFile(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        byte[] fileData;

        using (var ms = new MemoryStream())
        {
            stream.CopyTo(ms);
            fileData = ms.ToArray();
        }

        Data = new TpsRandomAccess(fileData, Encoding);
    }

    public RandomAccessTpsFile(Stream stream, Key key)
        : this(stream)
    {
        ArgumentNullException.ThrowIfNull(key);

        Decrypt(key);
    }

    public RandomAccessTpsFile(TpsRandomAccess rx)
    {
        Data = rx ?? throw new ArgumentNullException(nameof(rx));
    }

    public RandomAccessTpsFile(TpsRandomAccess rx, Key key)
        : this(rx)
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

    public override TpsFileHeader GetFileHeader()
    {
        Data.JumpAbsolute(0);

        var header = TpsFileHeader.Parse(Data);

        if (!header.IsTopSpeedFile)
        {
            throw new TpsParserException($"Not a TopSpeed file ({header.MagicNumber})");
        }

        return header;
    }

    public override IEnumerable<TpsBlock> GetBlocks()
    {
        var header = GetFileHeader();

        var blocks = header.BlockDescriptors
            // Skip zero-length pages and any blocks that are beyond the file size.
            .Where(range => !((range.Length == 0) || (range.StartOffset >= Data.Length)))

            .Select(range => TpsBlock.Parse(range, Data));

        return blocks;
    }

    private IEnumerable<TpsRecord> VisitRecords(bool ignoreErrors = false)
    {
        foreach (var block in GetBlocks())
        {
            foreach (var page in block.GetPages(ignoreErrors))
            {
                foreach (var record in page.GetRecords())
                {
                    yield return record;
                }
            }
        }
    }

    public override IEnumerable<IDataRecord> GetDataRows(int table, TableDefinition tableDefinition, bool ignoreErrors)
    {
        return VisitRecords(ignoreErrors)
            .Where(record => record.Payload is DataRecordPayload pl && pl.TableNumber == table)
            .Select(record => new DataRecord(record, tableDefinition, Encoding));
    }

    public override IEnumerable<DataRecordPayload> GetDataRecords(int table, TableDefinition tableDefinition, bool ignoreErrors)
    {
        return VisitRecords(ignoreErrors)
            .Where(record => record.Payload is DataRecordPayload pl && pl.TableNumber == table)
            .Select(record => (DataRecordPayload)record.Payload!);
    }

    public override IEnumerable<TableNameRecordPayload> GetTableNameRecords()
    {
        return VisitRecords()
            .Where(record => record.Payload is TableNameRecordPayload)
            .Select(record => (TableNameRecordPayload)record.Payload!);
    }

    public override IEnumerable<IndexRecordPayload> GetIndexes(int table, int index)
    {
        return VisitRecords()
            .Where(record =>
                (record.Payload is IndexRecordPayload payload)
                && payload.TableNumber == table
                && (payload.DefinitionIndex == index || index == -1))
            .Select(record => (IndexRecordPayload)record.Payload!);
    }

    public override IEnumerable<MetadataRecordPayload> GetMetadata(int table)
    {
        return VisitRecords()
            .Where(record => record.Payload is MetadataRecordPayload header && header.TableNumber == table)
            .Select(r => (MetadataRecordPayload)r.Payload!);
    }

    public override IEnumerable<TpsRecord> GetTpsRecords()
    {
        return VisitRecords();
    }

    private IEnumerable<MemoRecordPayload> OrderAndGroupMemos(IEnumerable<MemoRecordPayload> memoRecords)
    {
        // Records must be merged in order according to the memo's sequence number, so we order them.
        // Large memos are spread across multiple structures and must be joined later.
        var orderedBySequenceNumber = memoRecords
            .OrderBy(record => record.SequenceNumber);

        // Group the records by their owner and index.
        var groupedByOwnerAndIndex = orderedBySequenceNumber
            .GroupBy(record =>
            {
                //var header = (MemoHeader)record.Header;
                //return (owner: header.OwningRecord, index: header.MemoIndex);
                return (owner: record.RecordNumber, index: record.DefinitionIndex);
            });

        // Drop memos that have skipped sequence numbers, as this means the memo is missing a chunk of data.
        // Sequence numbers are zero-based.
        var filteredByCompleteSequences = groupedByOwnerAndIndex
            .Where(group => group.Count() - 1 == group.First().SequenceNumber);

        // Merge memo sequences into a single memo record.
        var resultingMemoRecords = filteredByCompleteSequences
            .Select(group => //new MemoRecord((MemoHeader)group.First().Header, Merge(group))
                group.First() with
                {
                    Content = MergeMemory(group.Select(r => r.Content)).ToArray()
                });

        return resultingMemoRecords;
    }

    public override IEnumerable<MemoRecordPayload> GetMemoRecords(int table, bool ignoreErrors)
    {
        var memoRecords = VisitRecords(ignoreErrors)
            .Where(record =>
                record.Payload is MemoRecordPayload payload
                && payload.TableNumber == table)
            .Select(r => (MemoRecordPayload)r.Payload!);

        return OrderAndGroupMemos(memoRecords);
    }

    public override IEnumerable<MemoRecordPayload> GetMemoRecords(int table, int memoIndex, bool ignoreErrors)
    {
        var memoRecords = VisitRecords(ignoreErrors)
            .Where(record =>
                record.Payload is MemoRecordPayload payload
                && payload.TableNumber == table
                && payload.DefinitionIndex == memoIndex)
            .Select(r => (MemoRecordPayload)r.Payload!);

        return OrderAndGroupMemos(memoRecords);
    }

    public override IReadOnlyDictionary<int, TableDefinition> GetTableDefinitions(bool ignoreErrors)
    {
        return VisitRecords(ignoreErrors)
            .Where(record => record.Payload is TableDefinitionRecordPayload)
            .Select(record => (TableDefinitionRecordPayload)record.Payload!)

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
                        Encoding))
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
