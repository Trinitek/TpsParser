using NUnit.Framework;
using System;
using TpsParser.Binary;
using TpsParser.Tps.Type;

namespace TpsParser.Tests.Tps.Type
{
    [TestFixture]
    public class TpsDateTest
    {
        [Test]
        public void ShouldReadFromRandomAccess()
        {
            var rx = new TpsRandomAccess(new byte[] { 0x10, 0x07, 0xE3, 0x07 });

            var date = new TpsDate(rx);

            Assert.That(date.Value, Is.EqualTo(new DateTime(2019, 7, 16)));
        }

        [Test]
        public void ShouldReadFromDateTime()
        {
            var dateTime = new DateTime(2019, 7, 16);

            var date = new TpsDate(dateTime);

            Assert.That(date.Value, Is.EqualTo(dateTime));
        }
    }
}
