using NUnit.Framework;
using System;
using TpsParser.Tps.Type;

namespace TpsParser.Tests.Tps.Type
{
    [TestFixture]
    public class TpsDateTest
    {
        [Test]
        public void ShouldReadFromRandomAccess()
        {
            var rx = new TpsReader(new byte[] { 0x10, 0x07, 0xE3, 0x07 });

            var date = rx.ReadTpsDate();

            Assert.AreEqual(new DateTime(2019, 7, 16), date.ToDateTime().Value);
        }

        [Test]
        public void ShouldReadFromDateTime()
        {
            var dateTime = new DateTime(2019, 7, 16);

            var date = new TpsDate(dateTime);

            Assert.AreEqual(dateTime, date.ToDateTime().Value);
        }
    }
}
