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
            MetadataEncoding = Encoding.ASCII
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
    public void MetadataEncoding_Keyword_SetToInvalidName_ShouldThrow()
    {
        var b = new TpsConnectionStringBuilder();

        Assert.That(
            () => b[TpsConnectionStringBuilder.MetadataEncodingName] = "foo",
            Throws.Exception.InstanceOf<ArgumentException>()
            .With.Message.Contains("is not recognized as a valid encoding"));
    }

    [Test]
    public void MetadataEncoding_Keyword_Null_ShouldClear()
    {
        var b = new TpsConnectionStringBuilder()
        {
            [TpsConnectionStringBuilder.MetadataEncodingName] = null
        };

        Assert.That(b.MetadataEncoding, Is.Null);
    }

    [Test]
    public void MetadataEncoding_Keyword_StringValue_ShouldSet()
    {
        var b = new TpsConnectionStringBuilder
        {
            [TpsConnectionStringBuilder.MetadataEncodingName] = "us-ascii"
        };

        Assert.That(b.MetadataEncoding, Is.EqualTo(Encoding.ASCII));
    }

    [Test]
    public void MetadataEncoding_Keyword_TypedValue_ShouldSet()
    {
        var b = new TpsConnectionStringBuilder
        {
            [TpsConnectionStringBuilder.MetadataEncodingName] = Encoding.ASCII
        };

        Assert.That(b.MetadataEncoding, Is.EqualTo(Encoding.ASCII));
    }

    [Test]
    public void MetadataEncoding_ShouldParseFromConnectionString()
    {
        var b = new TpsConnectionStringBuilder()
        {
            ConnectionString = "MetadataEncoding=us-ascii"
        };

        Assert.That(b.MetadataEncoding, Is.EqualTo(Encoding.ASCII));
    }
}
