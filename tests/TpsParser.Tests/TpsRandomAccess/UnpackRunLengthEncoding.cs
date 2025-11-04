using NUnit.Framework;
using System.Text;
using TpsParser.Binary;

namespace TpsParser.RandomAccess.Tests;

[TestFixture]
internal sealed class UnpackRunLengthEncoding
{
    [Test]
    public void ShouldUnpackRleSimpleBlock()
    {
        // Skip one, '1', repeat 7 '1'.

        var ra = new TpsRandomAccess([0x01, 0x31, 0x07], Encoding.ASCII).UnpackRunLengthEncoding();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(ra.Length, Is.EqualTo(8));
            
            var unpacked = ra.PeekBytes(ra.Length);

            Assert.That(unpacked, Is.EqualTo([
                0x31,
                0x31, 0x31, 0x31, 0x31, 0x31, 0x31, 0x31
                ]));
        }
    }

    [Test]
    public void ShouldUnpackRleDoubleBlock()
    {
        // Skip one, '1', repeat 7 '1', skip '2', '3', repeat '3' 3 times

        var ra = new TpsRandomAccess([0x01, 0x31, 0x07, 0x02, 0x32, 0x33, 0x03], Encoding.ASCII).UnpackRunLengthEncoding();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(ra.Length, Is.EqualTo(13));
            
            var unpacked = ra.PeekBytes(ra.Length);

            Assert.That(unpacked, Is.EqualTo([
                0x31,
                0x31, 0x31, 0x31, 0x31, 0x31, 0x31, 0x31,
                0x32,
                0x33,
                0x33, 0x33, 0x33
                ]));
        }
    }

    [Test]
    public void ShouldEndAfterSkip()
    {
        var ra = new TpsRandomAccess([0x01, 0x31], Encoding.ASCII).UnpackRunLengthEncoding();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(ra.Length, Is.EqualTo(1));
            
            var unpacked = ra.PeekBytes(ra.Length);

            Assert.That(unpacked, Is.EqualTo([
                0x31
                ]));
        }
    }

    [Test]
    public void ShouldEndAfterRepeat()
    {
        var ra = new TpsRandomAccess([0x01, 0x31, 0x07], Encoding.ASCII).UnpackRunLengthEncoding();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(ra.Length, Is.EqualTo(8));
            
            var unpacked = ra.PeekBytes(ra.Length);

            Assert.That(unpacked, Is.EqualTo([
                0x31,
                0x31, 0x31, 0x31, 0x31, 0x31, 0x31, 0x31
                ]));
        }
    }

    [Test]
    public void ShouldUnpackRleLongSkip()
    {
        var block = new byte[131];
        block[0] = 0x80;
        block[1] = 0x01;
        block[130] = 0x10;

        var ra = new TpsRandomAccess(block, Encoding.ASCII).UnpackRunLengthEncoding();

        Assert.That(ra.Length, Is.EqualTo(128 + 16));
    }

    [Test]
    public void ShouldUnpackRleLongRepeat()
    {
        var ra = new TpsRandomAccess([0x01, 0x31, 0x80, 0x01], Encoding.ASCII).UnpackRunLengthEncoding();

        Assert.That(ra.Length, Is.EqualTo(129));
    }
}
