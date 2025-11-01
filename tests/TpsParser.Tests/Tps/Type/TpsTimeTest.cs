using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text;
using TpsParser.Binary;
using TpsParser.Tps;

namespace TpsParser.TypeModel.Tests;

[TestFixture]
internal sealed class TpsTimeTest
{
    [Test]
    public void ShouldReadFromFile()
    {
        using (var stream = new FileStream("Resources/table-with-time.tps", FileMode.Open))
        {
            var file = new RandomAccessTpsFile(stream);

            var tableDef = file.GetTableDefinitions(false).First().Value;
            var record = file.GetDataRecords(1, tableDef, false).First();

            var valuePairs = record.GetFieldValuePairs();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(((TpsTime)valuePairs["ClockIn"]).ToTimeSpan().Value, Is.EqualTo(new TimeSpan(0, 6, 23, 15, 0)));
                Assert.That(((TpsTime)valuePairs["ClockOut"]).ToTimeSpan().Value, Is.EqualTo(new TimeSpan(0, 12, 59, 59, 0)));
            }
        }
    }

    [Test]
    public void ShouldReadFromRandomAccess()
    {
        var rx = new TpsRandomAccess([99, 59, 59, 12], Encoding.ASCII);

        var time = rx.ReadTpsTime();

        // 99/100 seconds = 990 milliseconds

        using (Assert.EnterMultipleScope())
        {
            Assert.That(time.Hours, Is.EqualTo(12));
            Assert.That(time.Minutes, Is.EqualTo(59));
            Assert.That(time.Seconds, Is.EqualTo(59));
            Assert.That(time.Centiseconds, Is.EqualTo(99));
            Assert.That(time.ToTimeSpan().Value, Is.EqualTo(new TimeSpan(0, 12, 59, 59, 990)));
        }
    }
}
