using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using TpsParser.Tps;

namespace TpsParser.TypeModel.Tests;

[TestFixture]
internal sealed class TestClaTime
{
    [Test]
    public void ShouldReadFromFile()
    {
        using (var stream = new FileStream("Resources/table-with-time.tps", FileMode.Open))
        {
            var file = new TpsFile(stream);

            var tableDef = file.GetTableDefinitions(false).First().Value;
            var record = file.GetDataRows(1, tableDef, false).First();

            var valuePairs = record.GetFieldValuePairs();

            using (Assert.EnterMultipleScope())
            {
                ClaTime clockIn = (ClaTime)valuePairs["ClockIn"];
                ClaTime clockOut = (ClaTime)valuePairs["ClockOut"];

                Assert.That(clockIn.Hours, Is.EqualTo(6));
                Assert.That(clockIn.Minutes, Is.EqualTo(23));
                Assert.That(clockIn.Seconds, Is.EqualTo(15));
                Assert.That(clockIn.Centiseconds, Is.EqualTo(0));

                Assert.That(clockOut.Hours, Is.EqualTo(12));
                Assert.That(clockOut.Minutes, Is.EqualTo(59));
                Assert.That(clockOut.Seconds, Is.EqualTo(59));
                Assert.That(clockOut.Centiseconds, Is.EqualTo(0));

                Assert.That(clockIn.ToTimeOnly().Value, Is.EqualTo(new TimeOnly(6, 23, 15, 0)));
                Assert.That(clockOut.ToTimeOnly().Value, Is.EqualTo(new TimeOnly(12, 59, 59, 0)));
            }
        }
    }
}
