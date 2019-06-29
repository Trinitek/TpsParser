using NUnit.Framework;
using System.IO;
using System.Linq;
using TpsParser.Tps;

namespace TpsParser.Tests
{
    [TestFixture]
    public class TpsFileTest
    {
        private TpsFile File { get; set; }

        [SetUp]
        public void SetUp()
        {
            using (var stream = new FileStream("Resources/table.tps", FileMode.Open))
            {
                File = new TpsFile(stream);
            }
        }

        [Test]
        public void ShouldParseFile()
        {
            var records = File.GetAllRecords();

            Assert.AreEqual(10, records.Count());
        }

        [Test]
        public void ShouldParseTableMetadata()
        {
            Assert.AreEqual(1, File.GetTableNameRecords().Count());

            var tableDefinitions = File.GetTableDefinitions(ignoreErrors: false);

            Assert.AreEqual(1, tableDefinitions.Count());
            Assert.AreEqual(2, tableDefinitions[1].Fields.Count());
            Assert.AreEqual(2, tableDefinitions[1].Indexes.Count());
            Assert.AreEqual(0, tableDefinitions[1].Memos.Count());
        }

        [Test]
        public void ShouldParseTableFieldInfo()
        {
            var tableDefinitions = File.GetTableDefinitions(ignoreErrors: false);
            var fields = tableDefinitions[1].Fields;

            Assert.AreEqual("CON1:OUDNR", fields[0].FullName);
            Assert.AreEqual("OUDNR", fields[0].Name);
            Assert.AreEqual("SIGNED-SHORT", fields[0].TypeName);

            Assert.AreEqual("CON1:NEWNR", fields[1].FullName);
            Assert.AreEqual("NEWNR", fields[1].Name);
            Assert.AreEqual("SIGNED-SHORT", fields[1].TypeName);
        }

        [Test]
        public void ShouldParseRecord()
        {
            var tableDefinitions = File.GetTableDefinitions(ignoreErrors: false);
            var dataRecords = File.GetDataRecords(table: 1, tableDefinitions[1], ignoreErrors: false)
                .ToList();

            Assert.AreEqual(1, dataRecords.Count());
            Assert.AreEqual(2, dataRecords[0].RecordNumber);
            Assert.AreEqual(2, dataRecords[0].Values.Count());
            Assert.AreEqual(1, dataRecords[0].Values.ToList()[0]);
            Assert.AreEqual(1, dataRecords[0].Values.ToList()[1]);
        }

        [Test]
        public void ShouldParseIndexData()
        {
            var indexes1 = File.GetIndexes(table: 1, index: 0)
                .ToList();

            Assert.AreEqual(1, indexes1.Count());
            Assert.AreEqual(2, indexes1[0].RecordNumber);

            var indexes2 = File.GetIndexes(table: 1, index: 1)
                .ToList();

            Assert.AreEqual(1, indexes2.Count());
            Assert.AreEqual(2, indexes2[0].RecordNumber);
        }
    }
}
