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
        (string baseFileName, string? tableName) = QueryParser.GetFileTableName(CommandText);

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

        var tableDefs = tpsFile.GetTableDefinitions();

        int tableNumber;
        TableDefinition tableDef;

        if (tableName is not null)
        {
            var tableNameRecords = tpsFile.GetTableNameRecordPayloads();

            var foundTableName = tableNameRecords.FirstOrDefault(r => string.Equals(
                r.GetName(encoding: tpsFile.EncodingOptions.MetadataEncoding),
                tableName,
                StringComparison.InvariantCultureIgnoreCase));

            if (foundTableName is null)
            {
                throw new InvalidOperationException($"Unable to locate table name '{tableName}' in file.");
            }

            tableNumber = foundTableName.TableNumber;
            tableDef = tableDefs[tableNumber];
        }
        else
        {
            var first = tableDefs.First();

            tableNumber = first.Key;
            tableDef = first.Value;
        }

        var ret = new TpsDataReader(
            tpsFile: tpsFile,
            tableDefinition: tableDef,
            tableNumber: tableNumber,
            fieldIteratorNodes: FieldValueReader.CreateFieldIteratorNodes(
                fieldDefinitions: tableDef.Fields,
                requestedFieldIndexes: [.. tableDef.Fields.Select(f => f.Index)]));
        
        ret.NextResult();

        return ret;
    }
}