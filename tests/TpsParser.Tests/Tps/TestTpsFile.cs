using NUnit.Framework;
using System.IO;
using System.Linq;
using TpsParser.TypeModel;

namespace TpsParser.Tests.Tps;

[TestFixture]
internal sealed class TestTpsFile
{
    private TpsFile GetTableFile()
    {
        using (var stream = new FileStream("Resources/table.tps", FileMode.Open))
        {
            return new TpsFile(stream);
        }
    }

    private TpsFile GetTableWithMemosFile()
    {
        using (var stream = new FileStream("Resources/table-with-memos.tps", FileMode.Open))
        {
            return new TpsFile(stream);
        }
    }

    [Test]
    public void ShouldParseFile()
    {
        var file = GetTableFile();

        var records = file.GetTpsRecords();

        Assert.That(records.Count(), Is.EqualTo(10));
    }

    [Test]
    public void ShouldParseTableMetadata()
    {
        var file = GetTableFile();

        var tableNames = file.GetTableNameRecordPayloads();

        Assert.That(tableNames.Count(), Is.EqualTo(1));

        var tableDefinitions = file.GetTableDefinitions(ignoreErrors: false);

        Assert.That(tableDefinitions, Has.Count.EqualTo(1));
        Assert.That(tableDefinitions[1].Fields, Has.Length.EqualTo(2));
        Assert.That(tableDefinitions[1].Indexes, Has.Length.EqualTo(2));
        Assert.That(tableDefinitions[1].Memos.Count, Is.Zero);
    }

    [Test]
    public void ShouldParseTableFieldInfo()
    {
        var file = GetTableFile();

        var tableDefinitions = file.GetTableDefinitions(ignoreErrors: false);
        var fields = tableDefinitions[1].Fields;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(fields[0].FullName, Is.EqualTo("CON1:OUDNR"));
            Assert.That(fields[0].Name, Is.EqualTo("OUDNR"));
            Assert.That(fields[0].TypeCode, Is.EqualTo(FieldTypeCode.Short));

            Assert.That(fields[1].FullName, Is.EqualTo("CON1:NEWNR"));
            Assert.That(fields[1].Name, Is.EqualTo("NEWNR"));
            Assert.That(fields[1].TypeCode, Is.EqualTo(FieldTypeCode.Short));
        }
    }

    [Test]
    public void ShouldParseRecord()
    {
        var file = GetTableFile();

        var tableDefinitions = file.GetTableDefinitions(ignoreErrors: false);
        var dataRecords = file.GetDataRows(table: 1, tableDefinitions[1], ignoreErrors: false)
            .ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(dataRecords, Has.Count.EqualTo(1));
            Assert.That(dataRecords[0].RecordNumber, Is.EqualTo(2));
            Assert.That(dataRecords[0].Values, Has.Count.EqualTo(2));
            Assert.That(((ClaShort)dataRecords[0].Values[0]).ToInt32().Value, Is.EqualTo(1));
            Assert.That(((ClaShort)dataRecords[0].Values[1]).ToInt32().Value, Is.EqualTo(1));
        }
    }

    [Test]
    public void ShouldParseIndexData()
    {
        var file = GetTableFile();

        var indexes1 = file.GetIndexRecordPayloads(table: 1, index: 0)
            .ToList();

        Assert.That(indexes1, Has.Count.EqualTo(1));
        Assert.That(indexes1[0].RecordNumber, Is.EqualTo(2));

        var indexes2 = file.GetIndexRecordPayloads(table: 1, index: 1)
            .ToList();

        Assert.That(indexes2, Has.Count.EqualTo(1));
        Assert.That(indexes2[0].RecordNumber, Is.EqualTo(2));
    }

    [Test]
    public void ShouldParseMemos()
    {
        var file = GetTableWithMemosFile();

        var tableDefinitions = file.GetTableDefinitions(ignoreErrors: false);
        var memos = file.GetMemoRecordPayloads(tableDefinitions.First().Key, ignoreErrors: false);

        Assert.That(memos.Count(), Is.EqualTo(5));
    }
}
