using NUnit.Framework;
using System;

namespace TpsParser.Data.Tests.TestTpsConnectionStringBuilder;

internal sealed class ErrorHandling
{
    [TestCase(Data.ErrorHandling.Default, ExpectedResult = "ErrorHandling=Default")]
    [TestCase(Data.ErrorHandling.Strict, ExpectedResult = "ErrorHandling=Strict")]
    public string ErrorHandling_Prop_ShouldSet(Data.ErrorHandling value)
    {
        var b = new TpsConnectionStringBuilder
        {
            ErrorHandling = value
        };

        return b.ConnectionString;
    }

    [Test]
    public void ErrorHandling_Prop_Null_ShouldSet()
    {
        var b = new TpsConnectionStringBuilder
        {
            ErrorHandling = null
        };

        Assert.That(b.ConnectionString, Is.EqualTo(""));
    }

    [Test]
    public void ErrorHandling_Keyword_SetToInvalidValue_ShouldThrow()
    {
        var b = new TpsConnectionStringBuilder();

        Assert.That(
            () => b[TpsConnectionStringBuilder.ErrorHandlingName] = "foo",
            Throws.Exception.InstanceOf<ArgumentException>()
            .With.Message.Contains("is not a valid")
            .With.Message.Contains(nameof(ErrorHandling)));
    }

    [Test]
    public void ErrorHandling_Keyword_Null_ShouldClear()
    {
        var b = new TpsConnectionStringBuilder
        {
            [TpsConnectionStringBuilder.ErrorHandlingName] = null
        };

        Assert.That(b.ErrorHandling, Is.Null);
    }

    [Test]
    public void ErrorHandling_Keyword_StringValue_ShouldSet()
    {
        var b = new TpsConnectionStringBuilder
        {
            [TpsConnectionStringBuilder.ErrorHandlingName] = "Default"
        };

        Assert.That(b.ErrorHandling, Is.EqualTo(Data.ErrorHandling.Default));
    }

    [Test]
    public void ErrorHandling_Keyword_TypedValue_ShouldSet()
    {
        var b = new TpsConnectionStringBuilder
        {
            [TpsConnectionStringBuilder.ErrorHandlingName] = Data.ErrorHandling.Default
        };

        Assert.That(b.ErrorHandling, Is.EqualTo(Data.ErrorHandling.Default));
    }

    [TestCase("", ExpectedResult = null)]
    [TestCase("ErrorHandling=Default", ExpectedResult = Data.ErrorHandling.Default)]
    [TestCase("ErrorHandling=Strict", ExpectedResult = Data.ErrorHandling.Strict)]
    public Data.ErrorHandling? ErrorHandling_ShouldParseFromConnectionString(string value)
    {
        var b = new TpsConnectionStringBuilder(value);

        return b.ErrorHandling;
    }

    
}
