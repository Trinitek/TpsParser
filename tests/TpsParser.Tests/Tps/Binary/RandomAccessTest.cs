using NUnit.Framework;
using System;
using System.Text;
using TpsParser.Binary;

namespace TpsParser.Tests.Tps.Binary
{
    [TestFixture]
    public class TpsReaderTest
    {
        [TestCase(0x01, new byte[] { 0x01 })]
        [TestCase(0x81, new byte[] { 0x81 })]
        public void ShouldParseByte(byte value, byte[] data)
        {
            var rx = new TpsReader(data);
            byte parsed = rx.Byte();

            Assert.AreEqual(value, parsed);
        }

        [TestCase((short)0x0102, new byte[] { 0x01, 0x02 })]
        [TestCase(unchecked((short)0x8182), new byte[] { 0x81, 0x82 })]
        public void ShouldParseShortBigEndian(short value, byte[] data)
        {
            var rx = new TpsReader(data);
            short parsed = rx.ShortBE();

            Assert.AreEqual(value, parsed);
        }

        [TestCase((short)0x0102, new byte[] { 0x02, 0x01 })]
        [TestCase(unchecked((short)0x8182), new byte[] { 0x82, 0x81 })]
        public void ShouldParseShortLittleEndian(short value, byte[] data)
        {
            var rx = new TpsReader(data);
            short parsed = rx.ShortLE();

            Assert.AreEqual(value, parsed);
        }

        [TestCase(unchecked((int)0x01020304), new byte[] { 0x01, 0x02, 0x03, 0x04 })]
        [TestCase(unchecked((int)0x81828384), new byte[] { 0x81, 0x82, 0x83, 0x84 })]
        public void ShouldParseLongBigEndian(int value, byte[] data)
        {
            var rx = new TpsReader(data);
            var parsed = rx.LongBE();

            Assert.AreEqual(value, parsed);
        }

        [TestCase(unchecked((int)0x01020304), new byte[] { 0x04, 0x03, 0x02, 0x01 })]
        [TestCase(unchecked((int)0x81828384), new byte[] { 0x84, 0x83, 0x82, 0x81 })]
        public void ShouldParseLongLittleEndian(int value, byte[] data)
        {
            var rx = new TpsReader(data);
            var parsed = rx.LongLE();

            Assert.AreEqual(value, parsed);
        }

        [Test]
        public void ShouldReadFixedLengthString()
        {
            Assert.AreEqual(" !", new TpsReader(new byte[] { 0x20, 0x21, 0x22 }).FixedLengthString(2));
        }

        [Test]
        public void ShouldReadFixedLengthStringWithCP850()
        {
            var codepage850 = CodePagesEncodingProvider.Instance.GetEncoding(850);

            Assert.AreEqual("é!", new TpsReader(new byte[] { 0x82, 0x21, 0x22 }).FixedLengthString(2, codepage850));
        }
        
        [Test]
        public void ShouldReadZeroTerminatedString()
        {
            Assert.AreEqual(" !\"", new TpsReader(new byte[] { 0x20, 0x21, 0x22, 0x00, 0x23 }).ZeroTerminatedString());
        }

        [Test]
        public void ShouldToAscii()
        {
            Assert.AreEqual(".!\" 00 #", new TpsReader(new byte[] { 0x20, 0x21, 0x22, 0x00, 0x23 }).ToAscii());
        }

        [Test]
        public void ShouldFloat()
        {
            Assert.AreEqual(0.0f, new TpsReader(new byte[] { 0x00, 0x00, 0x00, 0x00 }).FloatLE(), delta: 0.0);
            Assert.IsTrue(float.IsInfinity(new TpsReader(new byte[] { 0x00, 0x00, 0x80, 0x7F }).FloatLE()));
            Assert.IsTrue(float.IsInfinity(new TpsReader(new byte[] { 0x00, 0x00, 0x80, 0xFF }).FloatLE()));
            Assert.AreEqual(0.1f, new TpsReader(new byte[] { 0xCD, 0xCC, 0xCC, 0x3D }).FloatLE());
        }

        [Test]
        public void ShouldDouble()
        {
            Assert.AreEqual(0.0d, new TpsReader(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }).DoubleLE(), delta: 0.0);
            Assert.IsTrue(double.IsInfinity(new TpsReader(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x7F }).DoubleLE()));
            Assert.IsTrue(double.IsInfinity(new TpsReader(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0xFF }).DoubleLE()));
            Assert.AreEqual(0.1d, new TpsReader(new byte[] { 0x9a, 0x99, 0x99, 0x99, 0x99, 0x99, 0xB9, 0x3F }).DoubleLE(), delta: 0.0);
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
            var rx = new TpsReader(data);
            var bcd = rx.BinaryCodedDecimal(bcdLength, bcdDigitsAfterDecimal);

            Assert.AreEqual(value, bcd);
        }

        [Test]
        public void LEReadWriteTest()
        {
            var ra = new TpsReader(new byte[] { 1, 2, 3, 4 });
            int value = ra.LongLE();
            ra.JumpAbsolute(0);
            ra.WriteLongLE(value);
            ra.JumpAbsolute(0);
            int value2 = ra.LongLE();

            Assert.AreEqual(value, value2);
        }

        [Test]
        public void ShouldReuseBuffer()
        {
            var ra = new TpsReader(new byte[] { 1, 2, 3, 4 });
            ra.Byte();
            var read = ra.Read(3);

            Assert.AreEqual(3, read.Length);
            Assert.AreEqual(0, read.Position);
            Assert.AreEqual(2, read.Byte());
            Assert.AreEqual(3, read.Byte());
            Assert.AreEqual(4, read.Byte());

            read.JumpAbsolute(0);
            Assert.AreEqual(2, read.Byte());

            read.JumpRelative(1);
            Assert.AreEqual(4, read.Byte());

            read.JumpAbsolute(1);

            var read2 = read.Read(2);

            Assert.AreEqual(2, read2.Length);
            Assert.AreEqual(0, read2.Position);
            Assert.AreEqual(3, read2.Byte());
            Assert.AreEqual(4, read2.Byte());
        }

        [Test]
        public void ShouldFailBeyondBuffer()
        {
            var ra = new TpsReader(new byte[] { 1, 2, 3, 4 });
            ra.Byte();
            var read = ra.Read(3);

            Assert.Throws<IndexOutOfRangeException>(() => read.LongLE());
        }

        [Test]
        public void ShouldFailBeforeBuffer()
        {
            var ra = new TpsReader(new byte[] { 1, 2, 3, 4 });
            ra.Byte();
            var read = ra.Read(3);
            read.JumpAbsolute(-1);

            Assert.Throws<IndexOutOfRangeException>(() => read.Byte());
        }
    }
}
