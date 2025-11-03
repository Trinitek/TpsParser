using NUnit.Framework;
using System.IO;
using TpsParser.Tps;

namespace TpsParser.Tests.Tps;

[TestFixture]
internal sealed class TestTpsFileHeader
{
    [Test]
    public void ShouldParseHeader()
    {
        var file = new RandomAccessTpsFile(new FileStream("Resources/header.dat", FileMode.Open));

        var header = file.GetFileHeader();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(header.IsTopSpeedFile);
            Assert.That(header.FileLength1, Is.EqualTo(383744));
            Assert.That(header.LastIssuedRow, Is.EqualTo(5048));
            Assert.That(header.Changes, Is.EqualTo(15651));
            Assert.That(header.BlockDescriptors, Has.Length.EqualTo(60));
        }
    }

    [Test]
    public void ShouldNotParseHeaderIfNotTopSpeed()
    {
        var file = new RandomAccessTpsFile(new FileStream("Resources/bad-header.dat", FileMode.Open));

        Assert.That(() => file.GetFileHeader(), Throws.TypeOf<TpsParserException>().With.Message.Contains("not a TopSpeed file").IgnoreCase);
    }
}
