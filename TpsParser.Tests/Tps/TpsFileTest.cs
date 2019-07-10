using NUnit.Framework;
using System.IO;
using System.Linq;
using TpsParser.Tps;
using TpsParser.Tps.Type;

namespace TpsParser.Tests.Tps
{
    [TestFixture]
    public class TpsFileTest
    {
        private TpsFile GetTableFile()
        {
            using (var stream = new FileStream("Resources/table.tps", FileMode.Open))
            {
                return new TpsFile(stream);
            }
        }

        private TpsFile GetTableWithMemosFile()
        {
            using (var stream = new FileStream("Resources/table-with-memos.tps", FileMode.Open))
            {
                return new TpsFile(stream);
            }
        }

        [Test]
        public void ShouldParseFile()
        {
            var file = GetTableFile();

            var records = file.GetAllRecords();

            Assert.AreEqual(10, records.Count());
        }

        [Test]
        public void ShouldParseTableMetadata()
        {
            var file = GetTableFile();

            var tableNames = file.GetTableNameRecords();

            Assert.AreEqual(1, tableNames.Count());

            var tableDefinitions = file.GetTableDefinitions(ignoreErrors: false);

            Assert.AreEqual(1, tableDefinitions.Count());
            Assert.AreEqual(2, tableDefinitions[1].Fields.Count());
            Assert.AreEqual(2, tableDefinitions[1].Indexes.Count());
            Assert.AreEqual(0, tableDefinitions[1].Memos.Count());
        }

        [Test]
        public void ShouldParseTableFieldInfo()
        {
            var file = GetTableFile();

            var tableDefinitions = file.GetTableDefinitions(ignoreErrors: false);
            var fields = tableDefinitions[1].Fields;

            Assert.AreEqual("CON1:OUDNR", fields[0].FullName);
            Assert.AreEqual("OUDNR", fields[0].Name);
            Assert.AreEqual(TpsTypeCode.Short, fields[0].Type);

            Assert.AreEqual("CON1:NEWNR", fields[1].FullName);
            Assert.AreEqual("NEWNR", fields[1].Name);
            Assert.AreEqual(TpsTypeCode.Short, fields[1].Type);
        }

        [Test]
        public void ShouldParseRecord()
        {
            var file = GetTableFile();

            var tableDefinitions = file.GetTableDefinitions(ignoreErrors: false);
            var dataRecords = file.GetDataRecords(table: 1, tableDefinitions[1], ignoreErrors: false)
                .ToList();

            Assert.AreEqual(1, dataRecords.Count());
            Assert.AreEqual(2, dataRecords[0].RecordNumber);
            Assert.AreEqual(2, dataRecords[0].Values.Count());
            Assert.AreEqual(1, dataRecords[0].Values.ToList()[0].Value);
            Assert.AreEqual(1, dataRecords[0].Values.ToList()[1].Value);
        }

        [Test]
        public void ShouldParseIndexData()
        {
            var file = GetTableFile();

            var indexes1 = file.GetIndexes(table: 1, index: 0)
                .ToList();

            Assert.AreEqual(1, indexes1.Count());
            Assert.AreEqual(2, indexes1[0].RecordNumber);

            var indexes2 = file.GetIndexes(table: 1, index: 1)
                .ToList();

            Assert.AreEqual(1, indexes2.Count());
            Assert.AreEqual(2, indexes2[0].RecordNumber);
        }

        [Test]
        public void ShouldParseMemos()
        {
            var file = GetTableWithMemosFile();

            var tableDefinitions = file.GetTableDefinitions(ignoreErrors: false);
            var memos = file.GetMemoRecords(tableDefinitions.First().Key, ignoreErrors: false);

            Assert.AreEqual(2, memos.Count());
        }
    }
}
