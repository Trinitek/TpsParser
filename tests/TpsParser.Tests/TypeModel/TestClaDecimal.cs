using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace TpsParser.TypeModel.Tests;

[TestFixture]
internal sealed class TestClaDecimal
{
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
    public void ShouldThrowWhenStringIsEmptyOrWhitespace(string? value)
    {
        Assert.Throws<ArgumentException>(() => ClaDecimal.Parse(value!));
    }

    [TestCaseSource(typeof(ShouldConvertToDecimalData), nameof(ShouldConvertToDecimalData.TestCases))]
    public void ShouldConvertToDecimal(string value, decimal expected)
    {
        var dec = ClaDecimal.Parse(value);

        using (Assert.EnterMultipleScope())
        {
            var converted = dec.ToDecimal();

            Assert.That(converted.HasValue);
            Assert.That(converted.Value, Is.EqualTo(expected));
        }
    }

    private sealed class ShouldConvertToDecimalData
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
