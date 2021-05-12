using NUnit.Framework;
using System.IO;
using TpsParser.Tps;

namespace TpsParser.Tests.Tps
{
    [TestFixture]
    public class TpsHeaderTest
    {
        [Test]
        public void ShouldParseHeader()
        {
            var file = new RandomAccessTpsFile(new FileStream("Resources/header.dat", FileMode.Open), Parser.DefaultEncoding);

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
            var file = new RandomAccessTpsFile(new FileStream("Resources/bad-header.dat", FileMode.Open), Parser.DefaultEncoding);

            Assert.Throws<NotATopSpeedFileException>(() => file.GetHeader());
        }
    }
}
