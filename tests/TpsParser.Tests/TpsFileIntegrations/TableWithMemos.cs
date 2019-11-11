using NUnit.Framework;
using System.IO;
using System.Linq;
using TpsParser.Tps;

namespace TpsParser.Tests.TpsFileIntegrations
{
    [TestFixture]
    public class TableWithMemos
    {
        private TpsFile GetTableWithMemosFile()
        {
            using (var stream = new FileStream("Resources/table-with-memos.tps", FileMode.Open))
            {
                return new RandomAccessTpsFile(stream);
            }
        }

        [Test]
        public void ShouldParseMemos()
        {
            var file = GetTableWithMemosFile();

            var tableDefinitions = file.GetTableDefinitions(ignoreErrors: false);
            var memos = file.GetMemoRecords(tableDefinitions.First().Key, ignoreErrors: false);

            Assert.AreEqual(5, memos.Count());
        }
    }
}
