using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TpsParser.Binary;
using TpsParser.Tps.Header;
using TpsParser.Tps.Record;

namespace TpsParser.Tps
{
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


        public TpsFile(RandomAccess rx)
        {
            Data = rx ?? throw new ArgumentNullException(nameof(rx));
            Encoding = Encoding.GetEncoding("ISO-8859-1");
        }

        public TpsHeader GetHeader()
        {
            Data.JumpAbsolute(0);

            var header = new TpsHeader(Data);

            if (!header.IsTopSpeedFile)
            {
                throw new NotATopSpeedFileException($"Not a TopSpeed file ({header.TopSpeed})");
            }

            return header;
        }

        public IEnumerable<TpsBlock> GetBlocks(bool ignoreErrors)
        {
            var header = GetHeader();

            var blocks = Enumerable.Range(0, header.PageStart.Count)
                .Select(i => (offset: header.PageStart[i], end: header.PageEnd[i]))
                .Where(pair => ((pair.offset == 0x200) && (pair.end == 0x200)) || (pair.offset >= Data.Length))
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

        public IEnumerable<DataRecord> GetDataRecords(int table, TableDefinitionRecord tableDefinition, bool ignoreErrors)
        {
            return VisitRecords(ignoreErrors)
                .Where(record => record.Header is DataHeader && record.Header.TableNumber == table)
                .Select(record => new DataRecord(record, tableDefinition));
        }

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

        public IEnumerable<TpsRecord> GetMetadata(int table)
        {
            return VisitRecords()
                .Where(record => record.Header is MetadataHeader header && header.TableNumber == table);
        }

        public IEnumerable<TpsRecord> GetAllRecords()
        {
            return VisitRecords();
        }

        public IEnumerable<MemoRecord> GetMemoRecords(int table, int memoIndex, bool ignoreErrors)
        {
            return VisitRecords(ignoreErrors)
                .Where(record => record.Header is MemoHeader header && header.IsApplicable(table, memoIndex))

                // Records must be merged in order according to the memo's sequence number.
                .OrderBy(record => ((MemoHeader)record.Header).SequenceNumber)

                // Group records by the owning record index.
                .GroupBy(record => ((MemoHeader)record.Header).OwningRecord)

                // Do not process groups that have skipped sequence numbers. (i.e. 0, 1, 3, 4)
                .Where(group => group.Count() == ((MemoHeader)group.Last().Header).SequenceNumber + 1)

                .Select(group => new MemoRecord((MemoHeader)group.First().Header, Merge(group)));
        }

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
