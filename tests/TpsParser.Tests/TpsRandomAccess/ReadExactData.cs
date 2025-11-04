using NUnit.Framework;
using System;
using System.Text;
using TpsParser.Binary;

namespace TpsParser.RandomAccess.Tests;

[TestFixture]
internal sealed class ReadExactData
{
    [TestCase(0x01, new byte[] { 0x01 })]
    [TestCase(0x81, new byte[] { 0x81 })]
    public void ShouldReadByte(byte value, byte[] data)
    {
        var rx = new TpsRandomAccess(data, Encoding.ASCII);
        byte parsed = rx.ReadByte();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(parsed, Is.EqualTo(value));
            Assert.That(rx.Position, Is.EqualTo(1));
            Assert.That(rx.IsAtEnd);
        }
    }

    [TestCase((short)0x0102, new byte[] { 0x01, 0x02 })]
    [TestCase(unchecked((short)0x8182), new byte[] { 0x81, 0x82 })]
    public void ShouldReadShortBE(short value, byte[] data)
    {
        var rx = new TpsRandomAccess(data, Encoding.ASCII);
        short parsed = rx.ReadShortBE();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(parsed, Is.EqualTo(value));
            Assert.That(rx.Position, Is.EqualTo(2));
            Assert.That(rx.IsAtEnd);
        }
    }

    [TestCase((short)0x0102, new byte[] { 0x02, 0x01 })]
    [TestCase(unchecked((short)0x8182), new byte[] { 0x82, 0x81 })]
    public void ShouldReadShortLE(short value, byte[] data)
    {
        var rx = new TpsRandomAccess(data, Encoding.ASCII);
        short parsed = rx.ReadShortLE();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(parsed, Is.EqualTo(value));
            Assert.That(rx.Position, Is.EqualTo(2));
            Assert.That(rx.IsAtEnd);
        }
    }

    [TestCase(unchecked(0x01020304), new byte[] { 0x01, 0x02, 0x03, 0x04 })]
    [TestCase(unchecked((int)0x81828384), new byte[] { 0x81, 0x82, 0x83, 0x84 })]
    public void ShouldReadLongBE(int value, byte[] data)
    {
        var rx = new TpsRandomAccess(data, Encoding.ASCII);
        var parsed = rx.ReadLongBE();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(parsed, Is.EqualTo(value));
            Assert.That(rx.Position, Is.EqualTo(4));
            Assert.That(rx.IsAtEnd);
        }
    }

    [TestCase(unchecked(0x01020304), new byte[] { 0x04, 0x03, 0x02, 0x01 })]
    [TestCase(unchecked((int)0x81828384), new byte[] { 0x84, 0x83, 0x82, 0x81 })]
    public void ShouldReadLongLE(int value, byte[] data)
    {
        var rx = new TpsRandomAccess(data, Encoding.ASCII);
        var parsed = rx.ReadLongLE();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(parsed, Is.EqualTo(value));
            Assert.That(rx.Position, Is.EqualTo(4));
            Assert.That(rx.IsAtEnd);
        }
    }

    [Test]
    public void ShouldReadFixedLengthString_Ascii()
    {
        var rx = new TpsRandomAccess([0x20, 0x21, 0x22], Encoding.ASCII);
        var parsed = rx.ReadFixedLengthString(2);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(parsed, Is.EqualTo(" !"));
            Assert.That(rx.Position, Is.EqualTo(2));
            Assert.That(rx.IsAtEnd);
        }
    }

    [Test]
    public void ShouldReadFixedLengthString_CP850()
    {
        var codepage850 = CodePagesEncodingProvider.Instance.GetEncoding(850);
        var rx = new TpsRandomAccess([0x82, 0x21, 0x22], codepage850);
        var parsed = rx.ReadFixedLengthString(2);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(parsed, Is.EqualTo("é!"));
            Assert.That(rx.Position, Is.EqualTo(2));
            Assert.That(rx.IsAtEnd);
        }
    }

    [Test]
    public void ShouldReadFixedLengthString_Ascii_OverrideWith_CP850()
    {
        var codepage850 = CodePagesEncodingProvider.Instance.GetEncoding(850);
        var rx = new TpsRandomAccess([0x82, 0x21, 0x22], Encoding.ASCII);
        var parsed = rx.ReadFixedLengthString(2, codepage850);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(parsed, Is.EqualTo("é!"));
            Assert.That(rx.Position, Is.EqualTo(2));
            Assert.That(rx.IsAtEnd);
        }
    }
    
    [Test]
    public void ShouldReadZeroTerminatedString_Ascii()
    {
        var rx = new TpsRandomAccess([0x20, 0x21, 0x22, 0x00, 0x23], Encoding.ASCII);
        var parsed = rx.ReadZeroTerminatedString();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(parsed, Is.EqualTo(" !\""));
            Assert.That(rx.Position, Is.EqualTo(4));
            Assert.That(rx.IsAtEnd);
        }
    }

    [Test]
    public void ShouldToAscii()
    {
        Assert.That(new TpsRandomAccess([0x20, 0x21, 0x22, 0x00, 0x23], Encoding.ASCII).ToAscii(), Is.EqualTo(".!\" 00 #"));
    }

    [Test]
    public void ShouldReadFloatLE_0point0()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(new TpsRandomAccess([0x00, 0x00, 0x00, 0x00], Encoding.ASCII).ReadFloatLE(), Is.EqualTo(0.0f).Within(0.0));
        }
    }

    [Test]
    public void ShouldReadFloatLE_Infinity_7F()
    {
        var rx = new TpsRandomAccess([0x00, 0x00, 0x80, 0x7F], Encoding.ASCII);
        var parsed = rx.ReadFloatLE();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(float.IsInfinity(parsed));
            Assert.That(rx.Position, Is.EqualTo(4));
            Assert.That(rx.IsAtEnd);
        }
    }

    [Test]
    public void ShouldReadFloatLE_Infinity_FF()
    {
        var rx = new TpsRandomAccess([0x00, 0x00, 0x80, 0xFF], Encoding.ASCII);
        var parsed = rx.ReadFloatLE();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(float.IsInfinity(parsed));
            Assert.That(rx.Position, Is.EqualTo(4));
            Assert.That(rx.IsAtEnd);
        }
    }

    [Test]
    public void ShouldReadFloatLE_0point1()
    {
        var rx = new TpsRandomAccess([0xCD, 0xCC, 0xCC, 0x3D], Encoding.ASCII);
        var parsed = rx.ReadFloatLE();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(parsed, Is.EqualTo(0.1f));
            Assert.That(rx.Position, Is.EqualTo(4));
            Assert.That(rx.IsAtEnd);
        }
    }

    [Test]
    public void ShouldReadDoubleLE_0point0()
    {
        var rx = new TpsRandomAccess([0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00], Encoding.ASCII);
        var parsed = rx.ReadDoubleLE();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(parsed, Is.Zero.Within(0.0));
            Assert.That(rx.Position, Is.EqualTo(8));
            Assert.That(rx.IsAtEnd);
        }
    }

    [Test]
    public void ShouldReadDoubleLE_Infinity_7F()
    {
        var rx = new TpsRandomAccess([0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x7F], Encoding.ASCII);
        var parsed = rx.ReadDoubleLE();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(double.IsInfinity(parsed));
            Assert.That(rx.Position, Is.EqualTo(8));
            Assert.That(rx.IsAtEnd);
        }
    }

    [Test]
    public void ShouldReadDoubleLE_Infinity_FF()
    {
        var rx = new TpsRandomAccess([0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0xFF], Encoding.ASCII);
        var parsed = rx.ReadDoubleLE();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(double.IsInfinity(parsed));
            Assert.That(rx.Position, Is.EqualTo(8));
            Assert.That(rx.IsAtEnd);
        }
    }

    [Test]
    public void ShouldReadDoubleLE_0point1()
    {
        var rx = new TpsRandomAccess([0x9a, 0x99, 0x99, 0x99, 0x99, 0x99, 0xB9, 0x3F], Encoding.ASCII);
        var parsed = rx.ReadDoubleLE();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(parsed, Is.EqualTo(0.1d).Within(0.0));
            Assert.That(rx.Position, Is.EqualTo(8));
            Assert.That(rx.IsAtEnd);
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
    public void ShouldReadClaDecimal(string value, int bcdLength, byte bcdDigitsAfterDecimal, byte[] data)
    {
        var rx = new TpsRandomAccess(data, Encoding.ASCII);
        var bcd = rx.ReadClaDecimal(bcdLength, bcdDigitsAfterDecimal);
        string bcdString = bcd.ToString();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(bcdString, Is.EqualTo(value));
            Assert.That(rx.Position, Is.EqualTo(data.Length));
            Assert.That(rx.IsAtEnd);
        }
    }

    [Test]
    public void ShouldWriteLE()
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

        using (Assert.EnterMultipleScope())
        {
            Assert.That(read.Length, Is.EqualTo(3));
            Assert.That(read.Position, Is.Zero);
            Assert.That(read.ReadByte(), Is.EqualTo(2));
            Assert.That(read.ReadByte(), Is.EqualTo(3));
            Assert.That(read.ReadByte(), Is.EqualTo(4));
        }

        read.JumpAbsolute(0);
        Assert.That(read.ReadByte(), Is.EqualTo(2));

        read.JumpRelative(1);
        Assert.That(read.ReadByte(), Is.EqualTo(4));

        read.JumpAbsolute(1);

        var read2 = read.Read(2);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(read2.Length, Is.EqualTo(2));
            Assert.That(read2.Position, Is.Zero);
            Assert.That(read2.ReadByte(), Is.EqualTo(3));
            Assert.That(read2.ReadByte(), Is.EqualTo(4));
        }
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
