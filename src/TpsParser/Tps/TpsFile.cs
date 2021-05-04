using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TpsParser.Tps.Header;
using TpsParser.Tps.Record;

namespace TpsParser.Tps
{
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
        private Encoding _encoding;

        /// <summary>
        /// Instantiates a new TpsFile.
        /// </summary>
        /// <param name="encoding">The encoding to use when reading string fields.</param>
        public TpsFile(Encoding encoding)
        {
            Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        /// <summary>
        /// Gets the file header.
        /// </summary>
        /// <returns></returns>
        public abstract TpsHeader GetHeader();

        /// <summary>
        /// Gets a list of blocks in the file.
        /// </summary>
        /// <param name="ignoreErrors">True if exceptions should not be thrown when unexpected data is encountered.</param>
        /// <returns></returns>
        public abstract IEnumerable<TpsBlock> GetBlocks(bool ignoreErrors);

        /// <summary>
        /// Gets a list of data records for the associated table and its table definition.
        /// </summary>
        /// <param name="table">The table from which to get the records.</param>
        /// <param name="tableDefinitionRecord">The table definition that describes the table schema.</param>
        /// <param name="ignoreErrors">True if exceptions should not be thrown when unexpected data is encountered.</param>
        /// <returns></returns>
        public abstract IEnumerable<IDataRecord> GetDataRecords(int table, ITableDefinitionRecord tableDefinitionRecord, bool ignoreErrors);

        /// <summary>
        /// Gets a list of table name records that describe the name of the tables included in the file.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<TableNameRecord> GetTableNameRecords();

        /// <summary>
        /// Gets a list of index records.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="index"></param>
        /// <returns></returns>
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
        public abstract IEnumerable<IMemoRecord> GetMemoRecords(int table, bool ignoreErrors);

        /// <summary>
        /// Gets a dictionary of memo and blob records for the associated table.
        /// </summary>
        /// <param name="table">The table number that owns the memo.</param>
        /// <param name="memoIndex">The index number of the memo in the record, zero-based. Records can have more than one memo.</param>
        /// <param name="ignoreErrors">True if exceptions should not be thrown when unexpected data is encountered.</param>
        /// <returns></returns>
        public abstract IEnumerable<IMemoRecord> GetMemoRecords(int table, int memoIndex, bool ignoreErrors);

        /// <summary>
        /// Gets a dictionary of table definitions and their associated table numbers.
        /// </summary>
        /// <param name="ignoreErrors">True if exceptions should not be thrown when unexpected data is encountered.</param>
        /// <returns></returns>
        public abstract IReadOnlyDictionary<int, ITableDefinitionRecord> GetTableDefinitions(bool ignoreErrors);
    }

    /// <inheritdoc/>
    internal sealed class RandomAccessTpsFile : TpsFile
    {
        private TpsReader Data { get; }

        public RandomAccessTpsFile(Stream stream, Encoding encoding)
            : base(encoding)
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

            Data = new TpsReader(fileData);
        }

        public RandomAccessTpsFile(Stream stream, Key key, Encoding encoding)
            : this(stream, encoding)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Decrypt(key);
        }

        public RandomAccessTpsFile(TpsReader rx, Encoding encoding)
            : base(encoding)
        {
            Data = rx ?? throw new ArgumentNullException(nameof(rx));
        }

        public RandomAccessTpsFile(TpsReader rx, Key key, Encoding encoding)
            : this(rx, encoding)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Decrypt(key);
        }

        private void Decrypt(Key key)
        {
            key.Decrypt(new TpsReader(Data, 0, 0x200));

            var header = GetHeader();

            for (int i = 0; i < header.PageStart.Count; i++)
            {
                int offset = header.PageStart[i];
                int end = header.PageEnd[i];

                if ((offset != 0x200 || end != 0x200) && offset < Data.Length)
                {
                    key.Decrypt(new TpsReader(Data, offset, end - offset));
                }
            }
        }

        public override TpsHeader GetHeader()
        {
            Data.JumpAbsolute(0);

            var header = new TpsHeader(Data);

            if (!header.IsTopSpeedFile)
            {
                throw new NotATopSpeedFileException($"Not a TopSpeed file ({header.MagicNumber})");
            }

            return header;
        }

        public override IEnumerable<TpsBlock> GetBlocks(bool ignoreErrors)
        {
            var header = GetHeader();

            var blocks = Enumerable.Range(0, header.PageStart.Count)
                .Select(i => (offset: header.PageStart[i], end: header.PageEnd[i]))

                // Skip the first entry (0 length) and any blocks that are beyond the file size
                .Where(pair => !(((pair.offset == 0x200) && (pair.end == 0x200)) || (pair.offset >= Data.Length)))

                .Select(pair => new TpsBlock(Data, pair.offset, pair.end, ignoreErrors));

            return blocks;
        }

        private IEnumerable<TpsRecord> VisitRecords(bool ignoreErrors = false)
        {
            foreach (var block in GetBlocks(ignoreErrors))
            {
                foreach (var page in block.Pages)
                {
                    page.ParseRecords();

                    foreach (var record in page.GetRecords())
                    {
                        yield return record;
                    }

                    page.Flush();
                }
            }
        }

        public override IEnumerable<IDataRecord> GetDataRecords(int table, ITableDefinitionRecord tableDefinition, bool ignoreErrors)
        {
            return VisitRecords(ignoreErrors)
                .Where(record => record.Header is DataHeader && record.Header.TableNumber == table)
                .Select(record => new DataRecord(record, tableDefinition));
        }

        public override IEnumerable<TableNameRecord> GetTableNameRecords()
        {
            return VisitRecords()
                .Where(record => record.Header is ITableNameHeader)
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

        private IEnumerable<IMemoRecord> OrderAndGroupMemos(IEnumerable<TpsRecord> memoRecords)
        {
            // Records must be merged in order according to the memo's sequence number, so we order them.
            // Large memos are spread across multiple structures and must be joined later.
            var orderedBySequenceNumber = memoRecords
                .OrderBy(record => ((IMemoHeader)record.Header).SequenceNumber);

            // Group the records by their owner and index.
            var groupedByOwnerAndIndex = orderedBySequenceNumber
                .GroupBy(record =>
                {
                    var header = (IMemoHeader)record.Header;
                    return (owner: header.OwningRecord, index: header.MemoIndex);
                });

            // Drop memos that have skipped sequence numbers, as this means the memo is missing a chunk of data.
            // Sequence numbers are zero-based.
            var filteredByCompleteSequences = groupedByOwnerAndIndex
                .Where(group => group.Count() - 1 == ((IMemoHeader)group.First().Header).SequenceNumber);

            // Merge memo sequences into a single memo record.
            var resultingMemoRecords = filteredByCompleteSequences
                .Select(group => new MemoRecord((IMemoHeader)group.First().Header, Merge(group)));

            return resultingMemoRecords;
        }

        public override IEnumerable<IMemoRecord> GetMemoRecords(int table, bool ignoreErrors)
        {
            var memoRecords = VisitRecords(ignoreErrors)
                .Where(record =>
                    record.Header is IMemoHeader header
                    && header.TableNumber == table);

            return OrderAndGroupMemos(memoRecords);
        }

        public override IEnumerable<IMemoRecord> GetMemoRecords(int table, int memoIndex, bool ignoreErrors)
        {
            var memoRecords = VisitRecords(ignoreErrors)
                .Where(record =>
                    record.Header is IMemoHeader header
                    && header.TableNumber == table
                    && header.MemoIndex == memoIndex);

            return OrderAndGroupMemos(memoRecords);
        }

        public override IReadOnlyDictionary<int, ITableDefinitionRecord> GetTableDefinitions(bool ignoreErrors)
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
                    elementSelector: group => (ITableDefinitionRecord)new TableDefinitionRecord(Merge(group), Encoding));
        }

        private TpsReader Merge(IEnumerable<TpsRecord> records) =>
            new TpsReader(records.SelectMany(r => r.Data.GetRemainder()).ToArray());
    }
}
