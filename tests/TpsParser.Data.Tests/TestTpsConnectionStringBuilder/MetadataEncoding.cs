using NUnit.Framework;
using System;
using System.Text;

namespace TpsParser.Data.Tests.TestTpsConnectionStringBuilder;

internal sealed class MetadataEncoding
{
    [Test]
    public void MetadataEncoding_Prop_ShouldSet()
    {
        var b = new TpsConnectionStringBuilder()
        {
            MetadataEncoding = Encoding.ASCII.WebName
        };

        Assert.That(b.ConnectionString, Is.EqualTo("MetadataEncoding=us-ascii"));
    }

    [Test]
    public void MetadataEncoding_Prop_Null_ShouldSet()
    {
        var b = new TpsConnectionStringBuilder()
        {
            MetadataEncoding = null
        };

        Assert.That(b.ConnectionString, Is.EqualTo(""));
    }

    [Test]
    public void MetadataEncoding_Keyword_SetToInvalidName_GetEncodingOptions_ShouldThrow()
    {
        var b = new TpsConnectionStringBuilder
        {
            ["MetadataEncoding"] = "foo"
        };

        Assert.That(
            b.GetEncodingOptions,
            Throws.Exception.InstanceOf<ArgumentException>()
            .With.Message.Contains("not a supported encoding name"));
    }

    [Test]
    public void MetadataEncoding_Keyword_Null_ShouldClear()
    {
        var b = new TpsConnectionStringBuilder()
        {
            ["MetadataEncoding"] = null
        };

        Assert.That(b.MetadataEncoding, Is.Null);
    }

    [Test]
    public void MetadataEncoding_Keyword_StringValue_ShouldSet()
    {
        var b = new TpsConnectionStringBuilder
        {
            ["MetadataEncoding"] = "us-ascii"
        };

        var encodingOptions = b.GetEncodingOptions();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(b.MetadataEncoding, Is.EqualTo("us-ascii"));
            Assert.That(encodingOptions.MetadataEncoding, Is.EqualTo(Encoding.ASCII));
        }
    }

    [Test]
    public void MetadataEncoding_Keyword_TypedValue_ShouldSet()
    {
        var b = new TpsConnectionStringBuilder
        {
            ["MetadataEncoding"] = Encoding.ASCII
        };

        var encodingOptions = b.GetEncodingOptions();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(b.MetadataEncoding, Is.EqualTo("us-ascii"));
            Assert.That(encodingOptions.MetadataEncoding, Is.EqualTo(Encoding.ASCII));
        }
    }

    [Test]
    public void MetadataEncoding_ShouldParseFromConnectionString()
    {
        var b = new TpsConnectionStringBuilder()
        {
            ConnectionString = "MetadataEncoding=us-ascii"
        };

        var encodingOptions = b.GetEncodingOptions();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(b.MetadataEncoding, Is.EqualTo("us-ascii"));
            Assert.That(encodingOptions.MetadataEncoding, Is.EqualTo(Encoding.ASCII));
        }
    }
}
