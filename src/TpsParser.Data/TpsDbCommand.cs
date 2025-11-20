using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;

namespace TpsParser.Data;

public partial class TpsDbCommand : DbCommand
{
    public override string CommandText { get; set; }
    public override int CommandTimeout { get; set; }
    public override CommandType CommandType { get; set; }
    public override bool DesignTimeVisible { get; set; }
    public override UpdateRowSource UpdatedRowSource { get; set; }
    protected override DbConnection? DbConnection { get; set; }
    protected override DbParameterCollection DbParameterCollection { get; }
    protected override DbTransaction? DbTransaction { get; set; }

    public override void Cancel() {
        
    }

    public override int ExecuteNonQuery()
    {
        throw new NotImplementedException();
    }

    public override object? ExecuteScalar()
    {
        throw new NotImplementedException();
    }

    public override void Prepare()
    {
        // Do nothing
    }

    protected override DbParameter CreateDbParameter()
    {
        throw new NotImplementedException();
    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        var baseFileName = QueryParser.GetFileName(CommandText);

        string[] filenames =
        [
            $@"{baseFileName}.tps",
            $@"{baseFileName}",
        ];

        var folder = (Connection as TpsDbConnection)?.Database;
        var foundFile = default(string?);

        if (folder is { })
        {
            foreach(var filename in filenames)
            {
                var Path = System.IO.Path.Combine(folder, filename);

                if (File.Exists(Path))
                {
                    foundFile = Path;
                    break;
                }
            }
        }

        if (foundFile is null)
        {
            throw new FileNotFoundException($@"Unable to locate database file '{baseFileName}'.");
        }

        using var fs = new FileStream(foundFile, FileMode.Open);

        var tpsFile = new TpsFile(fs);

        var tableDef = tpsFile.GetTableDefinitions().First();

        var ret = new TpsDataReader(
            tpsFile: tpsFile,
            tableDefinition: tableDef.Value,
            tableNumber: tableDef.Key,
            fieldIteratorNodes: FieldValueReader.CreateFieldIteratorNodes(
                fieldDefinitions: tableDef.Value.Fields,
                requestedFieldIndexes: [.. tableDef.Value.Fields.Select(f => f.Index)]));
        
        ret.NextResult();

        return ret;
    }
}