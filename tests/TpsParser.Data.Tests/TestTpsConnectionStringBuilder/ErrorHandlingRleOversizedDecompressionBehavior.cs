using NUnit.Framework;
using System;

namespace TpsParser.Data.Tests.TestTpsConnectionStringBuilder;

internal sealed class ErrorHandlingRleOversizedDecompressionBehavior
{
    [TestCase(RleSizeMismatchBehavior.Throw, ExpectedResult = "ErrorHandling.RleOversizedDecompressionBehavior=Throw")]
    [TestCase(RleSizeMismatchBehavior.Skip, ExpectedResult = "ErrorHandling.RleOversizedDecompressionBehavior=Skip")]
    [TestCase(RleSizeMismatchBehavior.Allow, ExpectedResult = "ErrorHandling.RleOversizedDecompressionBehavior=Allow")]
    public string ErrorHandlingRleOversizedDecompressionBehavior_Prop_ShouldSet(RleSizeMismatchBehavior value)
    {
        var b = new TpsConnectionStringBuilder()
        {
            ErrorHandlingRleOversizedDecompressionBehavior = value
        };

        return b.ConnectionString;
    }

    [Test]
    public void ErrorHandlingRleOversizedDecompressionBehavior_Prop_Null_ShouldSet()
    {
        var b = new TpsConnectionStringBuilder()
        {
            ErrorHandlingRleOversizedDecompressionBehavior = null
        };

        Assert.That(b.ConnectionString, Is.EqualTo(""));
    }

    [Test]
    public void ErrorHandlingRleOversizedDecompressionBehavior_Keyword_SetToInvalidName_ShouldThrow()
    {
        var b = new TpsConnectionStringBuilder();

        Assert.That(
            () => b[TpsConnectionStringBuilder.ErrorHandlingRleOversizedDecompressionBehaviorName] = "foo",
            Throws.Exception.InstanceOf<ArgumentException>()
            .With.Message.Contains("is not a valid")
            .With.Message.Contains(nameof(RleSizeMismatchBehavior)));
    }

    [Test]
    public void ErrorHandlingRleOversizedDecompressionBehavior_Keyword_Null_ShouldClear()
    {
        var b = new TpsConnectionStringBuilder
        {
            [TpsConnectionStringBuilder.ErrorHandlingRleOversizedDecompressionBehaviorName] = null
        };

        Assert.That(b.ErrorHandlingRleOversizedDecompressionBehavior, Is.Null);
    }

    [Test]
    public void ErrorHandlingRleOversizedDecompressionBehavior_Keyword_StringValue_ShouldSet()
    {
        var b = new TpsConnectionStringBuilder
        {
            [TpsConnectionStringBuilder.ErrorHandlingRleOversizedDecompressionBehaviorName] = "Throw"
        };

        Assert.That(b.ErrorHandlingRleOversizedDecompressionBehavior, Is.EqualTo(RleSizeMismatchBehavior.Throw));
    }

    [Test]
    public void ErrorHandlingRleOversizedDecompressionBehavior_Keyword_TypedValue_ShouldSet()
    {
        var b = new TpsConnectionStringBuilder
        {
            [TpsConnectionStringBuilder.ErrorHandlingRleOversizedDecompressionBehaviorName] = RleSizeMismatchBehavior.Throw
        };

        Assert.That(b.ErrorHandlingRleOversizedDecompressionBehavior, Is.EqualTo(RleSizeMismatchBehavior.Throw));
    }

    [TestCase("", ExpectedResult = null)]
    [TestCase("ErrorHandling.RleOversizedDecompressionBehavior=Throw", ExpectedResult = RleSizeMismatchBehavior.Throw)]
    [TestCase("ErrorHandling.RleOversizedDecompressionBehavior=Skip", ExpectedResult = RleSizeMismatchBehavior.Skip)]
    [TestCase("ErrorHandling.RleOversizedDecompressionBehavior=Allow", ExpectedResult = RleSizeMismatchBehavior.Allow)]
    public RleSizeMismatchBehavior? ErrorHandlingRleOversizedDecompressionBehavior_ShouldParseFromConnectionString(string value)
    {
        var b = new TpsConnectionStringBuilder(value);

        return b.ErrorHandlingRleOversizedDecompressionBehavior;
    }
}
