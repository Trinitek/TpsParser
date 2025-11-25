using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TpsParser.Data.Tests;

internal sealed class TestTpsConnectionStringBuilder
{
    [TestCase(ErrorHandling.Default, ExpectedResult = "ErrorHandling=Default")]
    [TestCase(ErrorHandling.Strict, ExpectedResult = "ErrorHandling=Strict")]
    public string ErrorHandling_Prop_ShouldSet(ErrorHandling value)
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
            .With.Message.Contains(TpsConnectionStringBuilder.ErrorHandlingName));
    }

    [TestCase("", ExpectedResult = null)]
    [TestCase("ErrorHandling=Default", ExpectedResult = ErrorHandling.Default)]
    [TestCase("ErrorHandling=Strict", ExpectedResult = ErrorHandling.Strict)]
    public ErrorHandling? ShouldParseErrorHandling(string value)
    {
        var b = new TpsConnectionStringBuilder(value);

        return b.ErrorHandling;
    }
}
