using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TpsParser.Data.Tests.TestTpsConnectionStringBuilder;

internal sealed class FlattenCompoundStructureResults
{
    [Test]
    public void FlattenCompoundStructureResults_Prop_ShouldSet()
    {
        var b = new TpsConnectionStringBuilder()
        {
            FlattenCompoundStructureResults = true
        };

        Assert.That(b.ConnectionString, Is.EqualTo("FlattenCompoundStructureResults=True"));
    }

    [Test]
    public void FlattenCompoundStructureResults_Keyword_ShouldSet_True()
    {
        var b = new TpsConnectionStringBuilder()
        {
            ["FlattenCompoundStructureResults"] = "True"
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(b.FlattenCompoundStructureResults, Is.True);
            Assert.That(b.ConnectionString, Is.EqualTo("FlattenCompoundStructureResults=True"));
        }
    }

    [Test]
    public void FlattenCompoundStructureResults_Keyword_ShouldSet_False()
    {
        var b = new TpsConnectionStringBuilder()
        {
            ["FlattenCompoundStructureResults"] = "False"
        };
        using (Assert.EnterMultipleScope())
        {
            Assert.That(b.FlattenCompoundStructureResults, Is.False);
            Assert.That(b.ConnectionString, Is.EqualTo("FlattenCompoundStructureResults=False"));
        }
    }

    [Test]
    public void FlattenCompoundStructureResults_Keyword_ShouldSet_Invalid()
    {
        var b = new TpsConnectionStringBuilder();

        Assert.Throws<FormatException>(() =>
        {
            b["FlattenCompoundStructureResults"] = "InvalidBooleanValue";
        });
    }

    [Test]
    public void FlattenCompoundStructureResults_Prop_Null_ShouldClear()
    {
        var b = new TpsConnectionStringBuilder()
        {
            FlattenCompoundStructureResults = true
        };

        b.FlattenCompoundStructureResults = null;

        Assert.That(b.ConnectionString, Is.EqualTo(string.Empty));
    }

    [Test]
    public void FlattenCompoundStructureResults_Keyword_Null_ShouldClear()
    {
        var b = new TpsConnectionStringBuilder()
        {
            ["FlattenCompoundStructureResults"] = "True"
        };

        b["FlattenCompoundStructureResults"] = null;

        Assert.That(b.ConnectionString, Is.EqualTo(string.Empty));
    }

    [Test]
    public void FlattenCompoundStructureResults_ShouldParseFromConnectionString()
    {
        var b = new TpsConnectionStringBuilder("FlattenCompoundStructureResults=True");

        Assert.That(b.FlattenCompoundStructureResults, Is.True);
    }
}
