using NUnit.Framework;
using System;
using System.Collections.Generic;
using TpsParser.Binary;
using TpsParser.Tps.Type;
using TpsParser.TypeModel;

namespace TpsParser.Tests.Tps.Type;

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
    public void ShouldReadFromRandomAccess(string value, int bcdLength, int bcdDigitsAfterDecimal, byte[] data)
    {
        var rx = new TpsRandomAccess(data);
        var dec = new TpsDecimal(rx, bcdLength, bcdDigitsAfterDecimal);

        Assert.That(dec.Value, Is.EqualTo(value));
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
        var dec = new TpsDecimal(value);

        Assert.That(dec.Value, Is.EqualTo(value));
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("!")]
    [TestCase("-0. ")]
    [TestCase(" 2")]
    [TestCase("2..3")]
    [TestCase("2.3.")]
    [TestCase("--23")]
    public void ShouldThrowWhenStringIsMalformed(string value)
    {
        Assert.Throws<ArgumentException>(() => new TpsDecimal(value));
    }

    [Test]
    public void ShouldThrowWhenStringIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new TpsDecimal(null));
    }

    [TestCaseSource(typeof(ShouldConvertToDecimalData), nameof(ShouldConvertToDecimalData.TestCases))]
    public void ShouldConvertToDecimal(string value, decimal expected)
    {
        var dec = new TpsDecimal(value);

        Assert.That(((IConvertible<decimal>)dec).AsType(), Is.EqualTo(expected));
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
        var dec = new TpsDecimal(value);

        Assert.That(((IConvertible<bool>)dec).AsType(), Is.EqualTo(expected));
    }
}
