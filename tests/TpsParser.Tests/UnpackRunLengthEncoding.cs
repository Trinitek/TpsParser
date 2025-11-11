using NUnit.Framework;
using System.Text;

namespace TpsParser.Tests;

[TestFixture]
internal sealed class TestRleDecoder
{
    [Test]
    public void ShouldUnpackRleSimpleBlock()
    {
        // Skip one, '1', repeat 7 '1'.

        byte[] data = [0x01, 0x31, 0x07];

        var unpacked0 = RleDecoder.Unpack(data, expectedUnpackedSize: 8, Encoding.ASCII, ErrorHandlingOptions.Default);

        var ra = new TpsRandomAccess(unpacked0, Encoding.ASCII);

        //var ra = new TpsRandomAccess(, Encoding.ASCII).UnpackRunLengthEncoding();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(ra.Length, Is.EqualTo(8));
            
            var unpacked = ra.PeekBytes(ra.Length).ToArray();

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

        byte[] data = [0x01, 0x31, 0x07, 0x02, 0x32, 0x33, 0x03];

        //var ra = new TpsRandomAccess([0x01, 0x31, 0x07, 0x02, 0x32, 0x33, 0x03], Encoding.ASCII).UnpackRunLengthEncoding();

        var unpacked0 = RleDecoder.Unpack(data, expectedUnpackedSize: 13, Encoding.ASCII, ErrorHandlingOptions.Default);

        var ra = new TpsRandomAccess(unpacked0, Encoding.ASCII);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(ra.Length, Is.EqualTo(13));
            
            var unpacked = ra.PeekBytes(ra.Length).ToArray();

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
        byte[] data = [0x01, 0x31];

        //var ra = new TpsRandomAccess([0x01, 0x31], Encoding.ASCII).UnpackRunLengthEncoding();

        var unpacked0 = RleDecoder.Unpack(data, expectedUnpackedSize: 1, Encoding.ASCII, ErrorHandlingOptions.Default);

        var ra = new TpsRandomAccess(unpacked0, Encoding.ASCII);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(ra.Length, Is.EqualTo(1));
            
            var unpacked = ra.PeekBytes(ra.Length).ToArray();

            Assert.That(unpacked, Is.EqualTo([
                0x31
                ]));
        }
    }

    [Test]
    public void ShouldEndAfterRepeat()
    {
        byte[] data = [0x01, 0x31, 0x07];

        //var ra = new TpsRandomAccess([0x01, 0x31, 0x07], Encoding.ASCII).UnpackRunLengthEncoding();

        var unpacked0 = RleDecoder.Unpack(data, expectedUnpackedSize: 8, Encoding.ASCII, ErrorHandlingOptions.Default);

        var ra = new TpsRandomAccess(unpacked0, Encoding.ASCII);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(ra.Length, Is.EqualTo(8));
            
            var unpacked = ra.PeekBytes(ra.Length).ToArray();

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

        //var ra = new TpsRandomAccess(block, Encoding.ASCII).UnpackRunLengthEncoding();

        var unpacked0 = RleDecoder.Unpack(block, expectedUnpackedSize: 13, Encoding.ASCII, ErrorHandlingOptions.Default);

        var ra = new TpsRandomAccess(unpacked0, Encoding.ASCII);

        Assert.That(ra.Length, Is.EqualTo(128 + 16));
    }

    [Test]
    public void ShouldUnpackRleLongRepeat()
    {
        byte[] data = [0x01, 0x31, 0x80, 0x01];

        //var ra = new TpsRandomAccess([0x01, 0x31, 0x80, 0x01], Encoding.ASCII).UnpackRunLengthEncoding();

        var unpacked0 = RleDecoder.Unpack(data, expectedUnpackedSize: 13, Encoding.ASCII, ErrorHandlingOptions.Default);

        var ra = new TpsRandomAccess(unpacked0, Encoding.ASCII);

        Assert.That(ra.Length, Is.EqualTo(129));
    }
}
