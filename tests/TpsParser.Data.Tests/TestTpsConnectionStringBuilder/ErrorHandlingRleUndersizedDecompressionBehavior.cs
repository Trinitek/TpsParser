using NUnit.Framework;
using System;

namespace TpsParser.Data.Tests.TestTpsConnectionStringBuilder;

internal sealed class ErrorHandlingRleUndersizedDecompressionBehavior
{
    [TestCase(RleSizeMismatchBehavior.Throw, ExpectedResult = "ErrorHandling.RleUndersizedDecompressionBehavior=Throw")]
    [TestCase(RleSizeMismatchBehavior.Skip, ExpectedResult = "ErrorHandling.RleUndersizedDecompressionBehavior=Skip")]
    [TestCase(RleSizeMismatchBehavior.Allow, ExpectedResult = "ErrorHandling.RleUndersizedDecompressionBehavior=Allow")]
    public string ErrorHandlingRleUndersizedDecompressionBehavior_Prop_ShouldSet(RleSizeMismatchBehavior value)
    {
        var b = new TpsConnectionStringBuilder()
        {
            ErrorHandlingRleUndersizedDecompressionBehavior = value
        };

        return b.ConnectionString;
    }

    [Test]
    public void ErrorHandlingRleUndersizedDecompressionBehavior_Prop_Null_ShouldSet()
    {
        var b = new TpsConnectionStringBuilder()
        {
            ErrorHandlingRleUndersizedDecompressionBehavior = null
        };

        Assert.That(b.ConnectionString, Is.EqualTo(""));
    }

    [Test]
    public void ErrorHandlingRleUndersizedDecompressionBehavior_Keyword_SetToInvalidName_ShouldThrow()
    {
        var b = new TpsConnectionStringBuilder();

        Assert.That(
            () => b[TpsConnectionStringBuilder.ErrorHandlingRleUndersizedDecompressionBehaviorName] = "foo",
            Throws.Exception.InstanceOf<ArgumentException>()
            .With.Message.Contains("is not a valid")
            .With.Message.Contains(nameof(RleSizeMismatchBehavior)));
    }

    [Test]
    public void ErrorHandlingRleUndersizedDecompressionBehavior_Keyword_Null_ShouldClear()
    {
        var b = new TpsConnectionStringBuilder
        {
            [TpsConnectionStringBuilder.ErrorHandlingRleUndersizedDecompressionBehaviorName] = null
        };

        Assert.That(b.ErrorHandlingRleUndersizedDecompressionBehavior, Is.Null);
    }

    [Test]
    public void ErrorHandlingRleUndersizedDecompressionBehavior_Keyword_StringValue_ShouldSet()
    {
        var b = new TpsConnectionStringBuilder
        {
            [TpsConnectionStringBuilder.ErrorHandlingRleUndersizedDecompressionBehaviorName] = "Throw"
        };

        Assert.That(b.ErrorHandlingRleUndersizedDecompressionBehavior, Is.EqualTo(RleSizeMismatchBehavior.Throw));
    }

    [Test]
    public void ErrorHandlingRleUndersizedDecompressionBehavior_Keyword_TypedValue_ShouldSet()
    {
        var b = new TpsConnectionStringBuilder
        {
            [TpsConnectionStringBuilder.ErrorHandlingRleUndersizedDecompressionBehaviorName] = RleSizeMismatchBehavior.Throw
        };

        Assert.That(b.ErrorHandlingRleUndersizedDecompressionBehavior, Is.EqualTo(RleSizeMismatchBehavior.Throw));
    }

    [TestCase("", ExpectedResult = null)]
    [TestCase("ErrorHandling.RleUndersizedDecompressionBehavior=Throw", ExpectedResult = RleSizeMismatchBehavior.Throw)]
    [TestCase("ErrorHandling.RleUndersizedDecompressionBehavior=Skip", ExpectedResult = RleSizeMismatchBehavior.Skip)]
    [TestCase("ErrorHandling.RleUndersizedDecompressionBehavior=Allow", ExpectedResult = RleSizeMismatchBehavior.Allow)]
    public RleSizeMismatchBehavior? ErrorHandlingRleUndersizedDecompressionBehavior_ShouldParseFromConnectionString(string value)
    {
        var b = new TpsConnectionStringBuilder(value);

        return b.ErrorHandlingRleUndersizedDecompressionBehavior;
    }
}
