using NUnit.Framework;
using System;
using System.Text;
using TpsParser.Binary;
using TpsParser.Tps.Type;

namespace TpsParser.Tests.Tps.Type
{
    [TestFixture]
    public class TpsStringTest
    {
        [Test]
        public void ShouldReadFromTpsReader()
        {
            var rx = new TpsReader(new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F });

            var str = new TpsString(rx, Encoding.ASCII);

            Assert.AreEqual("Hello", str.Value);
        }

        [Test]
        public void ShouldReadFromString()
        {
            var str = new TpsString("Hello");

            Assert.AreEqual("Hello", str.Value);
        }

        [Test]
        public void ShouldThrowWhenStringCtorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TpsString(null));
        }
    }
}
