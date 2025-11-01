using NUnit.Framework;
using System;
using System.Text;
using TpsParser.Binary;

namespace TpsParser.Tests.Tps.Binary;

[TestFixture]
internal sealed class RandomAccessTest
{
    [TestCase(0x01, new byte[] { 0x01 })]
    [TestCase(0x81, new byte[] { 0x81 })]
    public void ShouldParseByte(byte value, byte[] data)
    {
        var rx = new TpsRandomAccess(data, Encoding.ASCII);
        byte parsed = rx.ReadByte();

        Assert.That(parsed, Is.EqualTo(value));
    }

    [TestCase((short)0x0102, new byte[] { 0x01, 0x02 })]
    [TestCase(unchecked((short)0x8182), new byte[] { 0x81, 0x82 })]
    public void ShouldParseShortBigEndian(short value, byte[] data)
    {
        var rx = new TpsRandomAccess(data, Encoding.ASCII);
        short parsed = rx.ReadShortBE();

        Assert.That(parsed, Is.EqualTo(value));
    }

    [TestCase((short)0x0102, new byte[] { 0x02, 0x01 })]
    [TestCase(unchecked((short)0x8182), new byte[] { 0x82, 0x81 })]
    public void ShouldParseShortLittleEndian(short value, byte[] data)
    {
        var rx = new TpsRandomAccess(data, Encoding.ASCII);
        short parsed = rx.ReadShortLE();

        Assert.That(parsed, Is.EqualTo(value));
    }

    [TestCase(unchecked((int)0x01020304), new byte[] { 0x01, 0x02, 0x03, 0x04 })]
    [TestCase(unchecked((int)0x81828384), new byte[] { 0x81, 0x82, 0x83, 0x84 })]
    public void ShouldParseLongBigEndian(int value, byte[] data)
    {
        var rx = new TpsRandomAccess(data, Encoding.ASCII);
        var parsed = rx.ReadLongBE();

        Assert.That(parsed, Is.EqualTo(value));
    }

    [TestCase(unchecked((int)0x01020304), new byte[] { 0x04, 0x03, 0x02, 0x01 })]
    [TestCase(unchecked((int)0x81828384), new byte[] { 0x84, 0x83, 0x82, 0x81 })]
    public void ShouldParseLongLittleEndian(int value, byte[] data)
    {
        var rx = new TpsRandomAccess(data, Encoding.ASCII);
        var parsed = rx.ReadLongLE();

        Assert.That(parsed, Is.EqualTo(value));
    }

    [Test]
    public void ShouldReadFixedLengthString_Ascii()
    {
        Assert.That(new TpsRandomAccess([0x20, 0x21, 0x22], Encoding.ASCII).ReadFixedLengthString(2), Is.EqualTo(" !"));
    }

    [Test]
    public void ShouldReadFixedLengthString_CP850()
    {
        var codepage850 = CodePagesEncodingProvider.Instance.GetEncoding(850);

        Assert.That(new TpsRandomAccess([0x82, 0x21, 0x22], codepage850).ReadFixedLengthString(2), Is.EqualTo("é!"));
    }

    [Test]
    public void ShouldReadFixedLengthString_Ascii_OverrideWith_CP850()
    {
        var codepage850 = CodePagesEncodingProvider.Instance.GetEncoding(850);

        Assert.That(new TpsRandomAccess([0x82, 0x21, 0x22], Encoding.ASCII).ReadFixedLengthString(2, codepage850), Is.EqualTo("é!"));
    }
    
    [Test]
    public void ShouldReadZeroTerminatedString_Ascii()
    {
        Assert.That(new TpsRandomAccess([0x20, 0x21, 0x22, 0x00, 0x23], Encoding.ASCII).ReadZeroTerminatedString(), Is.EqualTo(" !\""));
    }

    [Test]
    public void ShouldToAscii()
    {
        Assert.That(new TpsRandomAccess([0x20, 0x21, 0x22, 0x00, 0x23], Encoding.ASCII).ToAscii(), Is.EqualTo(".!\" 00 #"));
    }

    [Test]
    public void ShouldFloat()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(new TpsRandomAccess([0x00, 0x00, 0x00, 0x00], Encoding.ASCII).ReadFloatLE(), Is.EqualTo(0.0f).Within(0.0));
            Assert.That(float.IsInfinity(new TpsRandomAccess([0x00, 0x00, 0x80, 0x7F], Encoding.ASCII).ReadFloatLE()));
            Assert.That(float.IsInfinity(new TpsRandomAccess([0x00, 0x00, 0x80, 0xFF], Encoding.ASCII).ReadFloatLE()));
            Assert.That(new TpsRandomAccess([0xCD, 0xCC, 0xCC, 0x3D], Encoding.ASCII).ReadFloatLE(), Is.EqualTo(0.1f));
        }
    }

    [Test]
    public void ShouldDouble()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(new TpsRandomAccess([0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00], Encoding.ASCII).ReadDoubleLE(), Is.Zero.Within(0.0));
            Assert.That(double.IsInfinity(new TpsRandomAccess([0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x7F], Encoding.ASCII).ReadDoubleLE()));
            Assert.That(double.IsInfinity(new TpsRandomAccess([0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0xFF], Encoding.ASCII).ReadDoubleLE()));
            Assert.That(new TpsRandomAccess([0x9a, 0x99, 0x99, 0x99, 0x99, 0x99, 0xB9, 0x3F], Encoding.ASCII).ReadDoubleLE(), Is.EqualTo(0.1d).Within(0.0));
        }
    }

    [TestCase("0", 2, 0, new byte[] { 0x00, 0x00 })]
    [TestCase("979", 2, 0, new byte[] { 0x09, 0x79 })]
    [TestCase("0.00", 2, 2, new byte[] { 0x00, 0x00 })]
    [TestCase("10.0", 2, 1, new byte[] { 0x01, 0x00 })]
    [TestCase("0.0", 2, 1, new byte[] { 0x00, 0x00 })]
    [TestCase("0.00", 3, 2, new byte[] { 0x00, 0x00, 0x00 })]
    [TestCase("1.23", 2, 2, new byte[] { 0x01, 0x23 })]
    [TestCase("-1.23", 2, 2, new byte[] { 0xF1, 0x23 })]
    [TestCase("0.00000000", 7, 8, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })]
    [TestCase("0.50000", 3, 5, new byte[] { 0x05, 0x00, 0x00 })]
    public void ShouldParseBCD(string value, int bcdLength, byte bcdDigitsAfterDecimal, byte[] data)
    {
        var rx = new TpsRandomAccess(data, Encoding.ASCII);
        var bcd = rx.ReadTpsDecimal(bcdLength, bcdDigitsAfterDecimal);

        Assert.That(bcd.ToString(), Is.EqualTo(value));
    }

    [Test]
    public void LEReadWriteTest()
    {
        var ra = new TpsRandomAccess([1, 2, 3, 4], Encoding.ASCII);
        int value = ra.ReadLongLE();
        ra.JumpAbsolute(0);
        ra.WriteLongLE(value);
        ra.JumpAbsolute(0);
        int value2 = ra.ReadLongLE();

        Assert.That(value2, Is.EqualTo(value));
    }

    [Test]
    public void ShouldReuseBuffer()
    {
        var ra = new TpsRandomAccess([1, 2, 3, 4], Encoding.ASCII);
        ra.ReadByte();
        var read = ra.Read(3);

        Assert.That(read.Length, Is.EqualTo(3));
        Assert.That(read.Position, Is.Zero);
        Assert.That(read.ReadByte(), Is.EqualTo(2));
        Assert.That(read.ReadByte(), Is.EqualTo(3));
        Assert.That(read.ReadByte(), Is.EqualTo(4));

        read.JumpAbsolute(0);
        Assert.That(read.ReadByte(), Is.EqualTo(2));

        read.JumpRelative(1);
        Assert.That(read.ReadByte(), Is.EqualTo(4));

        read.JumpAbsolute(1);

        var read2 = read.Read(2);

        Assert.That(read2.Length, Is.EqualTo(2));
        Assert.That(read2.Position, Is.Zero);
        Assert.That(read2.ReadByte(), Is.EqualTo(3));
        Assert.That(read2.ReadByte(), Is.EqualTo(4));
    }

    [Test]
    public void ShouldFailBeyondBuffer()
    {
        var ra = new TpsRandomAccess([1, 2, 3, 4], Encoding.ASCII);
        ra.ReadByte();
        var read = ra.Read(3);

        Assert.Throws<IndexOutOfRangeException>(() => read.ReadLongLE());
    }

    [Test]
    public void ShouldFailBeforeBuffer()
    {
        var ra = new TpsRandomAccess([1, 2, 3, 4], Encoding.ASCII);
        ra.ReadByte();
        var read = ra.Read(3);
        read.JumpAbsolute(-1);

        Assert.Throws<IndexOutOfRangeException>(() => read.ReadByte());
    }
}
