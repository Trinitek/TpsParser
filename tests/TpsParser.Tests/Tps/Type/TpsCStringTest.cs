using NUnit.Framework;
using System;
using System.Text;
using TpsParser.Binary;
using TpsParser.Tps.Type;

namespace TpsParser.Tests.Tps.Type
{
    [TestFixture]
    public class TpsCStringTest
    {
        [Test]
        public void ShouldReadFromRandomAccess()
        {
            var rx = new TpsRandomAccess(new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x00 });

            var str = new TpsCString(rx, Encoding.ASCII);

            Assert.AreEqual("Hello", str.Value);
        }

        [Test]
        public void ShouldReadFromString()
        {
            var str = new TpsCString("Hello");

            Assert.AreEqual("Hello", str.Value);
        }

        [Test]
        public void ShouldThrowWhenStringCtorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TpsCString(null));
        }
    }
}
