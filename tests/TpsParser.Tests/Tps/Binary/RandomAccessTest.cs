using NUnit.Framework;
using System;
using System.Text;
using TpsParser.Binary;

namespace TpsParser.Tests.Tps.Binary
{
    [TestFixture]
    public class RandomAccessTest
    {
        [TestCase(0x01, new byte[] { 0x01 })]
        [TestCase(0x81, new byte[] { 0x81 })]
        public void ShouldParseByte(byte value, byte[] data)
        {
            var rx = new TpsRandomAccess(data);
            byte parsed = rx.Byte();

            Assert.That(parsed, Is.EqualTo(value));
        }

        [TestCase((short)0x0102, new byte[] { 0x01, 0x02 })]
        [TestCase(unchecked((short)0x8182), new byte[] { 0x81, 0x82 })]
        public void ShouldParseShortBigEndian(short value, byte[] data)
        {
            var rx = new TpsRandomAccess(data);
            short parsed = rx.ShortBE();

            Assert.That(parsed, Is.EqualTo(value));
        }

        [TestCase((short)0x0102, new byte[] { 0x02, 0x01 })]
        [TestCase(unchecked((short)0x8182), new byte[] { 0x82, 0x81 })]
        public void ShouldParseShortLittleEndian(short value, byte[] data)
        {
            var rx = new TpsRandomAccess(data);
            short parsed = rx.ShortLE();

            Assert.That(parsed, Is.EqualTo(value));
        }

        [TestCase(unchecked((int)0x01020304), new byte[] { 0x01, 0x02, 0x03, 0x04 })]
        [TestCase(unchecked((int)0x81828384), new byte[] { 0x81, 0x82, 0x83, 0x84 })]
        public void ShouldParseLongBigEndian(int value, byte[] data)
        {
            var rx = new TpsRandomAccess(data);
            var parsed = rx.LongBE();

            Assert.That(parsed, Is.EqualTo(value));
        }

        [TestCase(unchecked((int)0x01020304), new byte[] { 0x04, 0x03, 0x02, 0x01 })]
        [TestCase(unchecked((int)0x81828384), new byte[] { 0x84, 0x83, 0x82, 0x81 })]
        public void ShouldParseLongLittleEndian(int value, byte[] data)
        {
            var rx = new TpsRandomAccess(data);
            var parsed = rx.LongLE();

            Assert.That(parsed, Is.EqualTo(value));
        }

        [Test]
        public void ShouldReadFixedLengthString()
        {
            Assert.That(new TpsRandomAccess(new byte[] { 0x20, 0x21, 0x22 }).FixedLengthString(2), Is.EqualTo(" !"));
        }

        [Test]
        public void ShouldReadFixedLengthStringWithCP850()
        {
            var codepage850 = CodePagesEncodingProvider.Instance.GetEncoding(850);

            Assert.That(new TpsRandomAccess(new byte[] { 0x82, 0x21, 0x22 }).FixedLengthString(2, codepage850), Is.EqualTo("é!"));
        }
        
        [Test]
        public void ShouldReadZeroTerminatedString()
        {
            Assert.That(new TpsRandomAccess(new byte[] { 0x20, 0x21, 0x22, 0x00, 0x23 }).ZeroTerminatedString(), Is.EqualTo(" !\""));
        }

        [Test]
        public void ShouldToAscii()
        {
            Assert.That(new TpsRandomAccess(new byte[] { 0x20, 0x21, 0x22, 0x00, 0x23 }).ToAscii(), Is.EqualTo(".!\" 00 #"));
        }

        [Test]
        public void ShouldFloat()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(new TpsRandomAccess(new byte[] { 0x00, 0x00, 0x00, 0x00 }).FloatLE(), Is.EqualTo(0.0f).Within(0.0));
                Assert.That(float.IsInfinity(new TpsRandomAccess(new byte[] { 0x00, 0x00, 0x80, 0x7F }).FloatLE()));
                Assert.That(float.IsInfinity(new TpsRandomAccess(new byte[] { 0x00, 0x00, 0x80, 0xFF }).FloatLE()));
                Assert.That(new TpsRandomAccess(new byte[] { 0xCD, 0xCC, 0xCC, 0x3D }).FloatLE(), Is.EqualTo(0.1f));
            }
        }

        [Test]
        public void ShouldDouble()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(new TpsRandomAccess(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }).DoubleLE(), Is.Zero.Within(0.0));
                Assert.That(double.IsInfinity(new TpsRandomAccess(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x7F }).DoubleLE()));
                Assert.That(double.IsInfinity(new TpsRandomAccess(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0xFF }).DoubleLE()));
                Assert.That(new TpsRandomAccess(new byte[] { 0x9a, 0x99, 0x99, 0x99, 0x99, 0x99, 0xB9, 0x3F }).DoubleLE(), Is.EqualTo(0.1d).Within(0.0));
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
        public void ShouldParseBCD(string value, int bcdLength, int bcdDigitsAfterDecimal, byte[] data)
        {
            var rx = new TpsRandomAccess(data);
            var bcd = rx.BinaryCodedDecimal(bcdLength, bcdDigitsAfterDecimal);

            Assert.That(bcd, Is.EqualTo(value));
        }

        [Test]
        public void LEReadWriteTest()
        {
            var ra = new TpsRandomAccess(new byte[] { 1, 2, 3, 4 });
            int value = ra.LongLE();
            ra.JumpAbsolute(0);
            ra.WriteLongLE(value);
            ra.JumpAbsolute(0);
            int value2 = ra.LongLE();

            Assert.That(value2, Is.EqualTo(value));
        }

        [Test]
        public void ShouldReuseBuffer()
        {
            var ra = new TpsRandomAccess(new byte[] { 1, 2, 3, 4 });
            ra.Byte();
            var read = ra.Read(3);

            Assert.That(read.Length, Is.EqualTo(3));
            Assert.That(read.Position, Is.Zero);
            Assert.That(read.Byte(), Is.EqualTo(2));
            Assert.That(read.Byte(), Is.EqualTo(3));
            Assert.That(read.Byte(), Is.EqualTo(4));

            read.JumpAbsolute(0);
            Assert.That(read.Byte(), Is.EqualTo(2));

            read.JumpRelative(1);
            Assert.That(read.Byte(), Is.EqualTo(4));

            read.JumpAbsolute(1);

            var read2 = read.Read(2);

            Assert.That(read2.Length, Is.EqualTo(2));
            Assert.That(read2.Position, Is.Zero);
            Assert.That(read2.Byte(), Is.EqualTo(3));
            Assert.That(read2.Byte(), Is.EqualTo(4));
        }

        [Test]
        public void ShouldFailBeyondBuffer()
        {
            var ra = new TpsRandomAccess(new byte[] { 1, 2, 3, 4 });
            ra.Byte();
            var read = ra.Read(3);

            Assert.Throws<IndexOutOfRangeException>(() => read.LongLE());
        }

        [Test]
        public void ShouldFailBeforeBuffer()
        {
            var ra = new TpsRandomAccess(new byte[] { 1, 2, 3, 4 });
            ra.Byte();
            var read = ra.Read(3);
            read.JumpAbsolute(-1);

            Assert.Throws<IndexOutOfRangeException>(() => read.Byte());
        }
    }
}
