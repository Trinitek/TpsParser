using NUnit.Framework;

namespace TpsParser.Data.Tests.TestTpsConnectionStringBuilder;

internal sealed class DataSource
{
    [Test]
    public void DataSource_Prop_ShouldSet()
    {
        var b = new TpsConnectionStringBuilder()
        {
            DataSource = "path/to/file"
        };

        Assert.That(b.ConnectionString, Is.EqualTo("Data Source=path/to/file"));
    }

    [Test]
    public void Folder_Prop_ShouldSet()
    {
        var b = new TpsConnectionStringBuilder()
        {
            Folder = "path/to/file"
        };

        Assert.That(b.ConnectionString, Is.EqualTo("Data Source=path/to/file"));
    }

    [Test]
    public void DataSource_Keyword_ShouldSet()
    {
        var b = new TpsConnectionStringBuilder()
        {
            ["Data Source"] = "path/to/file"
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(b.DataSource, Is.EqualTo("path/to/file"));
            Assert.That(b.Folder, Is.EqualTo("path/to/file"));
            Assert.That(b.ConnectionString, Is.EqualTo("Data Source=path/to/file"));
        }
    }

    [Test]
    public void DataSourceWithoutSpace_Keyword_ShouldSet()
    {
        var b = new TpsConnectionStringBuilder()
        {
            ["DataSource"] = "path/to/file"
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(b.DataSource, Is.EqualTo("path/to/file"));
            Assert.That(b.Folder, Is.EqualTo("path/to/file"));
            Assert.That(b.ConnectionString, Is.EqualTo("Data Source=path/to/file"));
        }
    }

    [Test]
    public void Folder_Keyword_ShouldSet()
    {
        var b = new TpsConnectionStringBuilder()
        {
            ["Folder"] = "path/to/file"
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(b.DataSource, Is.EqualTo("path/to/file"));
            Assert.That(b.Folder, Is.EqualTo("path/to/file"));
            Assert.That(b.ConnectionString, Is.EqualTo("Data Source=path/to/file"));
        }
    }

    [Test]
    public void DataSource_Prop_Null_ShouldClear()
    {
        var b = new TpsConnectionStringBuilder()
        {
            DataSource = null
        };

        Assert.That(b.ConnectionString, Is.EqualTo(""));
    }

    [Test]
    public void Folder_Prop_Null_ShouldClear()
    {
        var b = new TpsConnectionStringBuilder()
        {
            Folder = null
        };

        Assert.That(b.ConnectionString, Is.EqualTo(""));
    }

    [Test]
    public void DataSource_Keyword_Null_ShouldClear()
    {
        var b = new TpsConnectionStringBuilder()
        {
            ["Data Source"] = null
        };

        Assert.That(b.ConnectionString, Is.EqualTo(""));
    }

    [Test]
    public void DataSourceWithoutSpace_Keyword_Null_ShouldClear()
    {
        var b = new TpsConnectionStringBuilder()
        {
            ["DataSource"] = null
        };

        Assert.That(b.ConnectionString, Is.EqualTo(""));
    }

    [Test]
    public void Folder_Keyword_Null_ShouldClear()
    {
        var b = new TpsConnectionStringBuilder()
        {
            ["Folder"] = null
        };

        Assert.That(b.ConnectionString, Is.EqualTo(""));
    }

    [Test]
    public void DataSource_ShouldParseFromConnectionString()
    {
        var b = new TpsConnectionStringBuilder("Data Source=path/to/file");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(b.DataSource, Is.EqualTo("path/to/file"));
            Assert.That(b.Folder, Is.EqualTo("path/to/file"));
        }
    }

    [Test]
    public void DataSourceWithoutSpace_ShouldParseFromConnectionString()
    {
        var b = new TpsConnectionStringBuilder("DataSource=path/to/file");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(b.DataSource, Is.EqualTo("path/to/file"));
            Assert.That(b.Folder, Is.EqualTo("path/to/file"));
        }
    }

    [Test]
    public void Folder_ShouldParseFromConnectionString()
    {
        var b = new TpsConnectionStringBuilder("Folder=path/to/file");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(b.DataSource, Is.EqualTo("path/to/file"));
            Assert.That(b.Folder, Is.EqualTo("path/to/file"));
        }
    }
}
