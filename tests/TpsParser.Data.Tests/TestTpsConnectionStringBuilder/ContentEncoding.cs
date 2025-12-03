using NUnit.Framework;
using System;
using System.Text;

namespace TpsParser.Data.Tests.TestTpsConnectionStringBuilder;

internal sealed class ContentEncoding
{
    [Test]
    public void ContentEncoding_Prop_ShouldSet()
    {
        var b = new TpsConnectionStringBuilder()
        {
            ContentEncoding = Encoding.ASCII.WebName
        };

        Assert.That(b.ConnectionString, Is.EqualTo("ContentEncoding=us-ascii"));
    }

    [Test]
    public void ContentEncoding_Prop_Null_ShouldSet()
    {
        var b = new TpsConnectionStringBuilder()
        {
            ContentEncoding = null
        };

        Assert.That(b.ConnectionString, Is.EqualTo(""));
    }

    [Test]
    public void ContentEncoding_Keyword_SetToInvalidName_GetEncodingOptions_ShouldThrow()
    {
        var b = new TpsConnectionStringBuilder
        {
            ["ContentEncoding"] = "foo"
        };

        Assert.That(
            b.GetEncodingOptions,
            Throws.Exception.InstanceOf<ArgumentException>()
            .With.Message.Contains("not a supported encoding name"));
    }

    [Test]
    public void ContentEncoding_Keyword_Null_ShouldClear()
    {
        var b = new TpsConnectionStringBuilder()
        {
            ["ContentEncoding"] = null
        };

        Assert.That(b.ContentEncoding, Is.Null);
    }

    [Test]
    public void ContentEncoding_Keyword_StringValue_ShouldSet()
    {
        var b = new TpsConnectionStringBuilder
        {
            ["ContentEncoding"] = "us-ascii"
        };

        var encodingOptions = b.GetEncodingOptions();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(b.ContentEncoding, Is.EqualTo("us-ascii"));
            Assert.That(encodingOptions.ContentEncoding, Is.EqualTo(Encoding.ASCII));
        }
    }

    [Test]
    public void ContentEncoding_Keyword_TypedValue_ShouldSet()
    {
        var b = new TpsConnectionStringBuilder
        {
            ["ContentEncoding"] = Encoding.ASCII
        };

        var encodingOptions = b.GetEncodingOptions();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(b.ContentEncoding, Is.EqualTo("us-ascii"));
            Assert.That(encodingOptions.ContentEncoding, Is.EqualTo(Encoding.ASCII));
        }
    }

    [Test]
    public void ContentEncoding_ShouldParseFromConnectionString()
    {
        var b = new TpsConnectionStringBuilder()
        {
            ConnectionString = "ContentEncoding=us-ascii"
        };

        var encodingOptions = b.GetEncodingOptions();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(b.ContentEncoding, Is.EqualTo("us-ascii"));
            Assert.That(encodingOptions.ContentEncoding, Is.EqualTo(Encoding.ASCII));
        }
    }
}
