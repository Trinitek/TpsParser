using NUnit.Framework;
using System.IO;

namespace TpsParser.Tests;

[TestFixture]
internal sealed class TestTpsFileHeader
{
    [Test]
    public void ShouldParseHeader()
    {
        var file = new TpsFile(new FileStream("Resources/header.dat", FileMode.Open));

        var header = file.GetFileHeader();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(header.IsTopSpeedFile);

            Assert.That(header.Address, Is.Zero);
            Assert.That(header.HeaderSize, Is.EqualTo(512));

            Assert.That(header.FileLength1, Is.EqualTo(383744));
            Assert.That(header.FileLength2, Is.EqualTo(383744));

            Assert.That(header.MagicNumber, Is.EqualTo("tOpS"));

            Assert.That(header.Zeroes, Is.Zero);
            Assert.That(header.LastIssuedRow, Is.EqualTo(5048));
            Assert.That(header.Changes, Is.EqualTo(15651));

            Assert.That(header.ManagementBlockOffset, Is.EqualTo(2304));

            Assert.That(header.BlockDescriptors, Has.Length.EqualTo(60));
            Assert.That(header.BlockDescriptors, Is.EqualTo((TpsBlockDescriptor[])
                [
                new(512, 512),
                new(512, 3840),
                new(3840, 138496),
                new(138496, 256000),

                new(256256, 321792),
                new(321792, 356352),
                new(357376, 368128),
                new(369408, 376576),

                new(377344, 381440),
                new(381440, 383744),
                new(383744, 383744),
                new(383744, 383744),

                new(383744, 383744), new(383744, 383744),
                new(383744, 383744), new(383744, 383744),
                new(383744, 383744), new(383744, 383744),
                new(383744, 383744), new(383744, 383744),

                new(383744, 383744), new(383744, 383744),
                new(383744, 383744), new(383744, 383744),
                new(383744, 383744), new(383744, 383744),
                new(383744, 383744), new(383744, 383744),

                new(383744, 383744), new(383744, 383744),
                new(383744, 383744), new(383744, 383744),
                new(383744, 383744), new(383744, 383744),
                new(383744, 383744), new(383744, 383744),

                new(383744, 383744), new(383744, 383744),
                new(383744, 383744), new(383744, 383744),
                new(383744, 383744), new(383744, 383744),
                new(383744, 383744), new(383744, 383744),

                new(383744, 383744), new(383744, 383744),
                new(383744, 383744), new(383744, 383744),
                new(383744, 383744), new(383744, 383744),
                new(383744, 383744), new(383744, 383744),

                new(383744, 383744), new(383744, 383744),
                new(383744, 383744), new(383744, 383744),
                new(383744, 383744), new(383744, 383744),
                new(383744, 383744), new(383744, 383744),
                ]));
        }
    }

    [Test]
    public void ShouldNotParseHeaderIfNotTopSpeed()
    {
        var file = new TpsFile(new FileStream("Resources/bad-header.dat", FileMode.Open));

        Assert.That(
            () => file.GetFileHeader(),
            Throws.TypeOf<TpsParserException>()
            .With.Message.Contains("not a TopSpeed file").IgnoreCase);
    }
}
