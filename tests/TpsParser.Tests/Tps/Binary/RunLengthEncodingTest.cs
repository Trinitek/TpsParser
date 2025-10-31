using NUnit.Framework;
using TpsParser.Binary;

namespace TpsParser.Tests.Tps.Binary;

[TestFixture]
internal sealed class RunLengthEncodingTest
{
    [Test]
    public void ShouldUnpackRleSimpleBlock()
    {
        // Skip one '1', repeat 7 '1'.

        var ra = new TpsRandomAccess(new byte[] { 0x01, 0x31, 0x07 }).UnpackRunLengthEncoding();

        Assert.That(ra.Length, Is.EqualTo(8));
        Assert.That(ra.ToAscii(), Is.EqualTo("11111111"));
    }

    [Test]
    public void ShouldUnpackRleDoubleBlock()
    {
        // Skip one '1', repeat 7 '1', skip '2', '3', repeat '3' 3 times

        var ra = new TpsRandomAccess(new byte[] { 0x01, 0x31, 0x07, 0x02, 0x32, 0x33, 0x03 }).UnpackRunLengthEncoding();

        Assert.That(ra.Length, Is.EqualTo(13));
        Assert.That(ra.ToAscii(), Is.EqualTo("1111111123333"));
    }

    [Test]
    public void ShouldEndAfterSkip()
    {
        var ra = new TpsRandomAccess(new byte[] { 0x01, 0x31 }).UnpackRunLengthEncoding();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(ra.Length, Is.EqualTo(1));
            Assert.That(ra.ToAscii(), Is.EqualTo("1"));
        }
    }

    [Test]
    public void ShouldEndAfterRepeat()
    {
        var ra = new TpsRandomAccess(new byte[] { 0x01, 0x31, 0x07 }).UnpackRunLengthEncoding();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(ra.Length, Is.EqualTo(8));
            Assert.That(ra.ToAscii(), Is.EqualTo("11111111"));
        }
    }

    [Test]
    public void ShouldUnpackRleLongSkip()
    {
        var block = new byte[131];
        block[0] = 0x80;
        block[1] = 0x01;
        block[130] = 0x10;

        var ra = new TpsRandomAccess(block).UnpackRunLengthEncoding();

        Assert.That(ra.Length, Is.EqualTo(128 + 16));
    }

    [Test]
    public void ShouldUnpackRleLongRepeat()
    {
        var ra = new TpsRandomAccess(new byte[] { 0x01, 0x31, 0x80, 0x01 }).UnpackRunLengthEncoding();

        Assert.That(ra.Length, Is.EqualTo(129));
    }
}
