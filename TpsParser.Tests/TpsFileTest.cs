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
    }
}
