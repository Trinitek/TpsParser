using NUnit.Framework;
using System.IO;
using TpsParser.Tps;

namespace TpsParser.Tests
{
    [TestFixture]
    public class TpsHeaderTest
    {
        [Test]
        public void ShouldParseHeader()
        {
            var file = new TpsFile(new FileStream("Resources/header.dat", FileMode.Open));

            var header = file.GetHeader();

            Assert.IsTrue(header.IsTopSpeedFile);
            Assert.AreEqual(383744, header.FileLength1);
            Assert.AreEqual(5048, header.LastIssuedRow);
            Assert.AreEqual(15651, header.Changes);
            Assert.AreEqual(60, header.PageStart.Count);
            Assert.AreEqual(60, header.PageEnd.Count);
        }

        [Test]
        public void ShouldNotParseHeaderIfNotTopSpeed()
        {
            var file = new TpsFile(new FileStream("Resources/bad-header.dat", FileMode.Open));

            Assert.Throws<NotATopSpeedFileException>(() => file.GetHeader());
        }
    }
}
