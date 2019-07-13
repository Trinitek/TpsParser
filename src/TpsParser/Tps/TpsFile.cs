using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TpsParser.Binary;
using TpsParser.Tps.Header;
using TpsParser.Tps.Record;

namespace TpsParser.Tps
{
    /// <summary>
    /// Represents a TopSpeed file and provides access to low level file and record structures.
    /// </summary>
    public sealed class TpsFile
    {
        private RandomAccess Data { get; }

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

        public TpsFile(Stream stream)
            : this()
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

            Data = new RandomAccess(fileData);
        }

        public TpsFile(Stream stream, Key key) 
            : this(stream)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Decrypt(key);
        }

        public TpsFile(RandomAccess rx)
            : this()
        {
            Data = rx ?? throw new ArgumentNullException(nameof(rx));
        }

        public TpsFile(RandomAccess rx, Key key)
            : this(rx)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Decrypt(key);
        }

        private TpsFile()
        {
            Encoding = Encoding.GetEncoding("ISO-8859-1");
        }

        private void Decrypt(Key key)
        {
            key.Decrypt(new RandomAccess(Data, 0, 0x200));

            var header = GetHeader();

            for (int i = 0; i < header.PageStart.Count; i++)
            {
                int offset = header.PageStart[i];
                int end = header.PageEnd[i];

                if ((offset != 0x200 || end != 0x200) && offset < Data.Length)
                {
                    key.Decrypt(new RandomAccess(Data, offset, end - offset));
                }
            }
        }

        public TpsHeader GetHeader()
        {
            Data.JumpAbsolute(0);

            var header = new TpsHeader(Data);

            if (!header.IsTopSpeedFile)
            {
                throw new NotATopSpeedFileException($"Not a TopSpeed file ({header.MagicNumber})");
            }

            return header;
        }

        public IEnumerable<TpsBlock> GetBlocks(bool ignoreErrors)
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

        /// <summary>
        /// Gets a list of data records for the associated table and its table definition.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="tableDefinition"></param>
        /// <param name="ignoreErrors"></param>
        /// <returns></returns>
        public IEnumerable<DataRecord> GetDataRecords(int table, TableDefinitionRecord tableDefinition, bool ignoreErrors)
        {
            return VisitRecords(ignoreErrors)
                .Where(record => record.Header is DataHeader && record.Header.TableNumber == table)
                .Select(record => new DataRecord(record, tableDefinition));
        }

        /// <summary>
        /// Gets a list of table name records that describe the name of the tables included in the file.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TableNameRecord> GetTableNameRecords()
        {
            return VisitRecords()
                .Where(record => record.Header is TableNameHeader)
                .Select(record => new TableNameRecord(record));
        }

        public IEnumerable<IndexRecord> GetIndexes(int table, int index)
        {
            return VisitRecords()
                .Where(record =>
                    (record.Header is IndexHeader header)
                    && header.TableNumber == table
                    && (header.IndexNumber == index || index == -1))
                .Select(record => new IndexRecord(record));
        }

        /// <summary>
        /// Gets a list of metadata that is included for the associated table.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public IEnumerable<TpsRecord> GetMetadata(int table)
        {
            return VisitRecords()
                .Where(record => record.Header is MetadataHeader header && header.TableNumber == table);
        }

        public IEnumerable<TpsRecord> GetAllRecords()
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

        /// <summary>
        /// Gets a dictionary of memo and blob records for the associated table.
        /// </summary>
        /// <param name="table">The table number that owns the memos.</param>
        /// <param name="ignoreErrors"></param>
        /// <returns></returns>
        public IEnumerable<MemoRecord> GetMemoRecords(int table, bool ignoreErrors)
        {
            var memoRecords = VisitRecords(ignoreErrors)
                .Where(record =>
                    record.Header is MemoHeader header
                    && header.TableNumber == table);

            return OrderAndGroupMemos(memoRecords);
        }

        /// <summary>
        /// Gets a dictionary of memo and blob records for the associated table.
        /// </summary>
        /// <param name="table">The table number that owns the memo.</param>
        /// <param name="memoIndex">The index number of the memo in the record, zero-based. Records can have more than one memo.</param>
        /// <param name="ignoreErrors"></param>
        /// <returns></returns>
        public IEnumerable<MemoRecord> GetMemoRecords(int table, int memoIndex, bool ignoreErrors)
        {
            var memoRecords = VisitRecords(ignoreErrors)
                .Where(record =>
                    record.Header is MemoHeader header
                    && header.TableNumber == table
                    && header.MemoIndex == memoIndex);

            return OrderAndGroupMemos(memoRecords);
        }

        /// <summary>
        /// Gets a list of table definitions for tables found in this file.
        /// </summary>
        /// <param name="ignoreErrors"></param>
        /// <returns></returns>
        public IReadOnlyDictionary<int, TableDefinitionRecord> GetTableDefinitions(bool ignoreErrors)
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
                elementSelector: group => new TableDefinitionRecord(Merge(group), Encoding));
        }

        private RandomAccess Merge(IEnumerable<TpsRecord> records) =>
            new RandomAccess(records.SelectMany(r => r.Data.GetRemainder()).ToArray());
    }
}
