using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TpsParser.Binary;
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
    public abstract IEnumerable<IDataRecord> GetDataRecords(int table, TableDefinitionRecord tableDefinitionRecord, bool ignoreErrors);

    /// <summary>
    /// Gets a list of table name records that describe the name of the tables included in the file.
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerable<TableNameRecord> GetTableNameRecords();

    public abstract IEnumerable<IndexRecord> GetIndexes(int table, int index);

    /// <summary>
    /// Gets a list of metadata that is included for the associated table.
    /// </summary>
    /// <param name="table">The table for which to get the metadata.</param>
    /// <returns></returns>
    public abstract IEnumerable<TpsRecord> GetMetadata(int table);

    /// <summary>
    /// Gets all of the records in the file.
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerable<TpsRecord> GetAllRecords();

    /// <summary>
    /// Gets a dictionary of memo and blob records for the associated table.
    /// </summary>
    /// <param name="table">The table number that owns the memos.</param>
    /// <param name="ignoreErrors">True if exceptions should not be thrown when unexpected data is encountered.</param>
    /// <returns></returns>
    public abstract IEnumerable<MemoRecord> GetMemoRecords(int table, bool ignoreErrors);

    /// <summary>
    /// Gets a dictionary of memo and blob records for the associated table.
    /// </summary>
    /// <param name="table">The table number that owns the memo.</param>
    /// <param name="memoIndex">The index number of the memo in the record, zero-based. Records can have more than one memo.</param>
    /// <param name="ignoreErrors">True if exceptions should not be thrown when unexpected data is encountered.</param>
    /// <returns></returns>
    public abstract IEnumerable<MemoRecord> GetMemoRecords(int table, int memoIndex, bool ignoreErrors);

    /// <summary>
    /// Gets a dictionary of table definitions and their associated table numbers.
    /// </summary>
    /// <param name="ignoreErrors">True if exceptions should not be thrown when unexpected data is encountered.</param>
    /// <returns></returns>
    public abstract IReadOnlyDictionary<int, TableDefinitionRecord> GetTableDefinitions(bool ignoreErrors);
}

/// <inheritdoc/>
internal sealed class RandomAccessTpsFile : TpsFile
{
    private TpsRandomAccess Data { get; }

    public RandomAccessTpsFile(Stream stream)
    {
        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

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
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        Decrypt(key);
    }

    public RandomAccessTpsFile(TpsRandomAccess rx)
    {
        Data = rx ?? throw new ArgumentNullException(nameof(rx));
    }

    public RandomAccessTpsFile(TpsRandomAccess rx, Key key)
        : this(rx)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        Decrypt(key);
    }

    private void Decrypt(Key key)
    {
        key.Decrypt(new TpsRandomAccess(Data, 0, 0x200));

        var header = GetFileHeader();

        foreach (var pageRange in header.BlockDescriptors)
        {
            int offset = pageRange.StartOffset;
            int end = pageRange.EndOffset;

            if ((offset != 0x200 || end != 0x200) && offset < Data.Length)
            {
                key.Decrypt(new TpsRandomAccess(Data, offset, end - offset));
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

    public override IEnumerable<IDataRecord> GetDataRecords(int table, TableDefinitionRecord tableDefinition, bool ignoreErrors)
    {
        return VisitRecords(ignoreErrors)
            .Where(record => record.Header is DataHeader && record.Header.TableNumber == table)
            .Select(record => new DataRecord(record, tableDefinition));
    }

    public override IEnumerable<TableNameRecord> GetTableNameRecords()
    {
        return VisitRecords()
            .Where(record => record.Header is TableNameHeader)
            .Select(record => new TableNameRecord(record));
    }

    public override IEnumerable<IndexRecord> GetIndexes(int table, int index)
    {
        return VisitRecords()
            .Where(record =>
                (record.Header is IndexHeader header)
                && header.TableNumber == table
                && (header.IndexNumber == index || index == -1))
            .Select(record => new IndexRecord(record));
    }

    public override IEnumerable<TpsRecord> GetMetadata(int table)
    {
        return VisitRecords()
            .Where(record => record.Header is MetadataHeader header && header.TableNumber == table);
    }

    public override IEnumerable<TpsRecord> GetAllRecords()
    {
        return VisitRecords();
    }

    private IEnumerable<MemoRecord> OrderAndGroupMemos(IEnumerable<TpsRecord> memoRecords)
    {
        // Records must be merged in order according to the memo's sequence number, so we order them.
        // Large memos are spread across multiple structures and must be joined later.
        var orderedBySequenceNumber = memoRecords
            .OrderBy(record => ((MemoHeader)record.Header).SequenceNumber);

        // Group the records by their owner and index.
        var groupedByOwnerAndIndex = orderedBySequenceNumber
            .GroupBy(record =>
            {
                var header = (MemoHeader)record.Header;
                return (owner: header.OwningRecord, index: header.MemoIndex);
            });

        // Drop memos that have skipped sequence numbers, as this means the memo is missing a chunk of data.
        // Sequence numbers are zero-based.
        var filteredByCompleteSequences = groupedByOwnerAndIndex
            .Where(group => group.Count() - 1 == ((MemoHeader)group.First().Header).SequenceNumber);

        // Merge memo sequences into a single memo record.
        var resultingMemoRecords = filteredByCompleteSequences
            .Select(group => new MemoRecord((MemoHeader)group.First().Header, Merge(group)));

        return resultingMemoRecords;
    }

    public override IEnumerable<MemoRecord> GetMemoRecords(int table, bool ignoreErrors)
    {
        var memoRecords = VisitRecords(ignoreErrors)
            .Where(record =>
                record.Header is MemoHeader header
                && header.TableNumber == table);

        return OrderAndGroupMemos(memoRecords);
    }

    public override IEnumerable<MemoRecord> GetMemoRecords(int table, int memoIndex, bool ignoreErrors)
    {
        var memoRecords = VisitRecords(ignoreErrors)
            .Where(record =>
                record.Header is MemoHeader header
                && header.TableNumber == table
                && header.MemoIndex == memoIndex);

        return OrderAndGroupMemos(memoRecords);
    }

    public override IReadOnlyDictionary<int, TableDefinitionRecord> GetTableDefinitions(bool ignoreErrors)
    {
        return VisitRecords(ignoreErrors)
            .Where(record => record.Header is TableDefinitionHeader)

            // Records must be merged in order according to the header's block index.
            .OrderBy(record => ((TableDefinitionHeader)record.Header).Block)

            // Group records by table number.
            .GroupBy(record => record.Header.TableNumber)

            // Do not process groups that have skipped block indexes. (i.e. 0, 1, 3, 4)
            .Where(group => group.Count() == ((TableDefinitionHeader)group.Last().Header).Block + 1)

            .ToDictionary(
            keySelector: group => group.Key,
            elementSelector: group => TableDefinitionRecord.Parse(Merge(group)));
    }

    private TpsRandomAccess Merge(IEnumerable<TpsRecord> records) =>
        new TpsRandomAccess(records.SelectMany(r => r.DataRx.GetRemainderAsByteArray()).ToArray(), Encoding);
}
