using NUnit.Framework;
using System;

namespace TpsParser.Data.Tests.TestTpsConnectionStringBuilder;

internal sealed class ErrorHandlingThrowOnRleDecompressionError
{
    [TestCase(false, ExpectedResult = "ErrorHandling.ThrowOnRleDecompressionError=False")]
    [TestCase(true, ExpectedResult = "ErrorHandling.ThrowOnRleDecompressionError=True")]
    public string ErrorHandlingThrowOnRleDecompressionError_Prop_ShouldSet(bool value)
    {
        var b = new TpsConnectionStringBuilder()
        {
            ErrorHandlingThrowOnRleDecompressionError = value
        };

        return b.ConnectionString;
    }

    [Test]
    public void ErrorHandlingThrowOnRleDecompressionError_Prop_Null_ShouldSet()
    {
        var b = new TpsConnectionStringBuilder()
        {
            ErrorHandlingThrowOnRleDecompressionError = null
        };

        Assert.That(b.ConnectionString, Is.EqualTo(""));
    }

    [Test]
    public void ErrorHandlingThrowOnRleDecompressionError_Keyword_SetToInvalidName_ShouldThrow()
    {
        var b = new TpsConnectionStringBuilder();

        Assert.That(
            () => b[TpsConnectionStringBuilder.ErrorHandlingThrowOnRleDecompressionErrorName] = "foo",
            Throws.Exception.InstanceOf<ArgumentException>()
            .With.Message.Contains("is not a valid")
            .With.Message.Contains("bool"));
    }

    [TestCase("", ExpectedResult = null)]
    [TestCase("ErrorHandling.ThrowOnRleDecompressionError=False", ExpectedResult = false)]
    [TestCase("ErrorHandling.ThrowOnRleDecompressionError=True", ExpectedResult = true)]
    public bool? ErrorHandlingThrowOnRleDecompressionError_ShouldParseFromConnectionString(string value)
    {
        var b = new TpsConnectionStringBuilder(value);

        return b.ErrorHandlingThrowOnRleDecompressionError;
    }
}
