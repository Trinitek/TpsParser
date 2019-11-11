using NUnit.Framework;
using System;
using TpsParser.Tests.DeserializerModels;
using TpsParser.Tps.Type;
using static TpsParser.Tests.RowDeserializer.RowDeserializerExtensions;

namespace TpsParser.Tests.RowDeserializer
{
    [TestFixture]
    public class DeserializeTime
    {
        [Test]
        public void ShouldDeserializeTime()
        {
            var time = new TimeSpan(12, 13, 42);

            var row = BuildRow(1, ("Time", new TpsTime(time)));

            var deserialized = row.Deserialize<TimeModel>();

            Assert.AreEqual(time, deserialized.Time);
        }

        [Test]
        public void ShouldDeserializeTimeFromLong()
        {
            int centiseconds = 80085;

            var row = BuildRow(1, ("Time", new TpsLong(centiseconds)));

            var deserialized = row.Deserialize<TimeModel>();

            Assert.AreEqual(new TimeSpan(0, 0, 13, 20, 850), deserialized.Time);
        }
    }
}
