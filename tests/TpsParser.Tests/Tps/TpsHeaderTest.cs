using NUnit.Framework;
using System.IO;
using TpsParser.Tps;

namespace TpsParser.Tests.Tps;

[TestFixture]
internal sealed class TpsHeaderTest
{
    [Test]
    public void ShouldParseHeader()
    {
        var file = new RandomAccessTpsFile(new FileStream("Resources/header.dat", FileMode.Open));

        var header = file.GetHeader();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(header.IsTopSpeedFile);
            Assert.That(header.FileLength1, Is.EqualTo(383744));
            Assert.That(header.LastIssuedRow, Is.EqualTo(5048));
            Assert.That(header.Changes, Is.EqualTo(15651));
            Assert.That(header.PageStart.Count, Is.EqualTo(60));
            Assert.That(header.PageEnd.Count, Is.EqualTo(60));
        }
    }

    [Test]
    public void ShouldNotParseHeaderIfNotTopSpeed()
    {
        var file = new RandomAccessTpsFile(new FileStream("Resources/bad-header.dat", FileMode.Open));

        Assert.Throws<NotATopSpeedFileException>(() => file.GetHeader());
    }
}
