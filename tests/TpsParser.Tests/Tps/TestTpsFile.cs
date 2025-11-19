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
            return new TpsFile(stream, errorHandlingOptions: ErrorHandlingOptions.Strict);
        }
    }

    private TpsFile GetTableWithMemosFile()
    {
        using (var stream = new FileStream("Resources/table-with-memos.tps", FileMode.Open))
        {
            return new TpsFile(stream, errorHandlingOptions: ErrorHandlingOptions.Strict);
        }
    }

    [Test]
    public void ShouldParseFile()
    {
        var file = GetTableFile();

        var records = file.EnumerateRecords();

        Assert.That(records.Count(), Is.EqualTo(10));
    }

    [Test]
    public void ShouldParseTableMetadata()
    {
        var file = GetTableFile();

        var tableNames = file.GetTableNameRecordPayloads();

        Assert.That(tableNames.Count(), Is.EqualTo(1));

        var tableDefinitions = file.GetTableDefinitions();

        Assert.That(tableDefinitions, Has.Count.EqualTo(1));
        Assert.That(tableDefinitions[1].Fields, Has.Length.EqualTo(2));
        Assert.That(tableDefinitions[1].Indexes, Has.Length.EqualTo(2));
        Assert.That(tableDefinitions[1].Memos.Count, Is.Zero);
    }

    [Test]
    public void ShouldParseTableFieldInfo()
    {
        var file = GetTableFile();

        var tableDefinitions = file.GetTableDefinitions();
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

        var tableDefinitions = file.GetTableDefinitions();
        var dataRecordPayloads = file.GetDataRecordPayloads(table: 1)
            .ToList();

        var nodes = FieldValueReader.CreateFieldIteratorNodes(
            fieldDefinitions: tableDefinitions[1].Fields,
            requestedFieldIndexes: [.. tableDefinitions[1].Fields.Select(f => f.Index)]);

        var dataRecord0 = FieldValueReader.EnumerateValues(nodes, dataRecordPayloads[0]).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(dataRecordPayloads, Has.Count.EqualTo(1));
            Assert.That(dataRecordPayloads[0].RecordNumber, Is.EqualTo(2));
            Assert.That(dataRecord0, Has.Count.EqualTo(2));
            Assert.That(((ClaShort)dataRecord0[0].Value).ToInt32().Value, Is.EqualTo(1));
            Assert.That(((ClaShort)dataRecord0[1].Value).ToInt32().Value, Is.EqualTo(1));
        }
    }

    [Test]
    public void ShouldParseIndexData()
    {
        var file = GetTableFile();

        var indexes1 = file.GetIndexRecordPayloads(table: 1, indexDefinitionIndex: 0)
            .ToList();

        Assert.That(indexes1, Has.Count.EqualTo(1));
        Assert.That(indexes1[0].RecordNumber, Is.EqualTo(2));

        var indexes2 = file.GetIndexRecordPayloads(table: 1, indexDefinitionIndex: 1)
            .ToList();

        Assert.That(indexes2, Has.Count.EqualTo(1));
        Assert.That(indexes2[0].RecordNumber, Is.EqualTo(2));
    }

    [Test]
    public void ShouldParseMemos()
    {
        var file = GetTableWithMemosFile();

        var tableDefinitions = file.GetTableDefinitions();
        var memos = file.GetTpsMemos(tableDefinitions.First().Key);

        Assert.That(memos.Count(), Is.EqualTo(5));
    }
}
