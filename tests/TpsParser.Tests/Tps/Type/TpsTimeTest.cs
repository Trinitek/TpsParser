using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using TpsParser.Binary;
using TpsParser.Tps;
using TpsParser.Tps.Type;

namespace TpsParser.Tests.Tps.Type
{
    [TestFixture]
    public class TpsTimeTest
    {
        [Test]
        public void ShouldReadFromFile()
        {
            using (var stream = new FileStream("Resources/table-with-time.tps", FileMode.Open))
            {
                var file = new TpsFile(stream);

                var tableDef = file.GetTableDefinitions(false).First().Value;
                var record = file.GetDataRecords(1, tableDef, false).First();

                var valuePairs = record.GetFieldValuePairs();

                Assert.AreEqual(new TimeSpan(0, 6, 23, 15, 0), valuePairs["ClockIn"].Value);
                Assert.AreEqual(new TimeSpan(0, 12, 59, 59, 0), valuePairs["ClockOut"].Value);
            }
        }

        [Test]
        public void ShouldReadFromRandomAccess()
        {
            var rx = new RandomAccess(new byte[] { 99, 59, 59, 12 });

            var time = new TpsTime(rx);

            // 99/100 seconds = 990 milliseconds

            Assert.AreEqual(new TimeSpan(0, 12, 59, 59, 990), time.Value);
        }
    }
}
