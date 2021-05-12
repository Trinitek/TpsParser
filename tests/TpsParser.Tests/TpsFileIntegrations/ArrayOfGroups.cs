using NUnit.Framework;
using System.IO;
using System.Linq;
using TpsParser.Tps;

namespace TpsParser.Tests.TpsFileIntegrations
{
    [TestFixture]
    public class ArrayOfGroups
    {
        private TpsFile GetTableFile()
        {
            using (var stream = new FileStream("Resources/array-of-groups.tps", FileMode.Open))
            {
                return new RandomAccessTpsFile(stream, Parser.DefaultEncoding);
            }
        }

        [Test]
        public void ShouldParseDataRecords()
        {
            var file = GetTableFile();

            var tableDef = file.GetTableDefinitions(ignoreErrors: false).First();
            var records = file.GetDataRecords(table: tableDef.Key, tableDefinitionRecord: tableDef.Value, ignoreErrors: false);

            Assert.AreEqual(1, records.Count());
        }
    }
}
