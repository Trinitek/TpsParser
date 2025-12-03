using NUnit.Framework;
using System;

namespace TpsParser.TypeModel.Tests;

[TestFixture]
internal sealed class TestClaDate
{
    [Test]
    public void ToDateOnly_ShouldReturn_DateOnly()
    {
        var dateOnly = new DateOnly(2019, 7, 16);

        var date = new ClaDate(dateOnly);

        Assert.That(date.ToDateOnly().Value, Is.EqualTo(dateOnly));
    }

    [Test]
    public void Date1801Jan01_AsClarionStandardDate_ShouldReturn_ClarionStandardDateMinValue()
    {
        var d = new ClaDate(new(1801, 1, 1));

        var result = d.AsClarionStandardDate();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.HasValue);

            if (!result.HasValue)
            {
                return;
            }

            Assert.That(result.Value.Value, Is.EqualTo(4));
            Assert.That(ClaDate.ClarionStandardDateMinValue, Is.EqualTo(4));
        }
    }

    [Test]
    public void Date1800Dec31_AsClarionStandardDate_ShouldReturn_None()
    {
        var d = new ClaDate(new(1800, 12, 31));

        var result = d.AsClarionStandardDate();

        Assert.That(result.HasValue, Is.False);
    }

    [Test]
    public void Date1801Jan02_AsClarionStandardDate_ShouldReturnValue()
    {
        var d = new ClaDate(new(1801, 1, 2));

        var result = d.AsClarionStandardDate();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.HasValue);

            if (!result.HasValue)
            {
                return;
            }

            Assert.That(result.Value.Value, Is.EqualTo(5));
        }
    }

    [Test]
    public void Date9999Dec31_AsClarionStandardDate_ShouldReturn_ClarionStandardDateMaxValue()
    {
        var d = new ClaDate(new(9999, 12, 31));

        var result = d.AsClarionStandardDate();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.HasValue);

            if (!result.HasValue)
            {
                return;
            }

            Assert.That(result.Value.Value, Is.EqualTo(2994626));
            Assert.That(ClaDate.ClarionStandardDateMaxValue, Is.EqualTo(2994626));
        }
    }

    [Test]
    public void Null_ToBoolean_ShouldReturn_False()
    {
        var d = new ClaDate(null);

        var result = d.ToBoolean();

        Assert.That(result, Is.False);
    }

    [Test]
    public void DateOnlyMinValue_ToBoolean_ShouldReturn_True()
    {
        var d = new ClaDate(DateOnly.MinValue);

        var result = d.ToBoolean();

        Assert.That(result, Is.True);
    }

    [Test]
    public void DateOnlyMaxValue_ToBoolean_ShouldReturn_True()
    {
        var d = new ClaDate(DateOnly.MaxValue);

        var result = d.ToBoolean();

        Assert.That(result, Is.True);
    }
}
