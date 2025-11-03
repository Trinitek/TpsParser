using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using TpsParser.Binary;

namespace TpsParser.TypeModel.Tests;

[TestFixture]
internal sealed class TpsDecimalTest
{
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
    public void ShouldReadFromRandomAccess(string value, int bcdLength, byte bcdDigitsAfterDecimal, byte[] data)
    {
        var rx = new TpsRandomAccess(data, Encoding.ASCII);
        var dec = rx.ReadClaDecimal(bcdLength, bcdDigitsAfterDecimal);

        Assert.That(dec.ToString(), Is.EqualTo(value));
    }

    [TestCase("0")]
    [TestCase("979")]
    [TestCase("0.00")]
    [TestCase("10.0")]
    [TestCase("0.0")]
    [TestCase("0.00")]
    [TestCase("1.23")]
    [TestCase("-1.23")]
    [TestCase("0.00000000")]
    [TestCase("0.50000")]
    public void ShouldReadFromString(string value)
    {
        var dec = ClaDecimal.Parse(value);

        Assert.That(dec.ToString(), Is.EqualTo(value));
    }

    [TestCase("!")]
    [TestCase("-0. ")]
    [TestCase(" 2")]
    [TestCase("2..3")]
    [TestCase("2.3.")]
    [TestCase("--23")]
    public void ShouldThrowWhenStringIsMalformed(string value)
    {
        Assert.Throws<FormatException>(() => ClaDecimal.Parse(value));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void ShouldThrowWhenStringIsEmptyOrWhitespace(string value)
    {
        Assert.Throws<ArgumentException>(() => ClaDecimal.Parse(value));
    }

    [TestCaseSource(typeof(ShouldConvertToDecimalData), nameof(ShouldConvertToDecimalData.TestCases))]
    public void ShouldConvertToDecimal(string value, decimal expected)
    {
        var dec = ClaDecimal.Parse(value);

        using (Assert.EnterMultipleScope())
        {
            var converted = dec.ToDecimal();

            Assert.That(converted.HasValue);

            if (converted.HasValue)
            {
                return;
            }

            Assert.That(converted.Value, Is.EqualTo(expected));
        }
    }

    private class ShouldConvertToDecimalData
    {
        public static IEnumerable<TestCaseData> TestCases
        {
            get
            {
                yield return new TestCaseData("0", 0m);
                yield return new TestCaseData("979", 979m);
                yield return new TestCaseData("0.00", 0m);
                yield return new TestCaseData("10.0", 10m);
                yield return new TestCaseData("0.0", 0m);
                yield return new TestCaseData("0.00", 0m);
                yield return new TestCaseData("1.23", 1.23m);
                yield return new TestCaseData("-1.23", -1.23m);
                yield return new TestCaseData("0.00000000", 0m);
                yield return new TestCaseData("0.50000", 0.5m);
            }
        }
    }

    [TestCase("0", false)]
    [TestCase("979", true)]
    [TestCase("0.00", false)]
    [TestCase("10.0", true)]
    [TestCase("0.0", false)]
    [TestCase("0.00", false)]
    [TestCase("1.23", true)]
    [TestCase("-1.23", true)]
    [TestCase("0.00000000", false)]
    [TestCase("0.50000", true)]
    public void ShouldConvertToBoolean(string value, bool expected)
    {
        var dec = ClaDecimal.Parse(value);

        Assert.That(dec.ToBoolean(), Is.EqualTo(expected));
    }
}
