using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;

namespace TpsParser.Data;

public partial class TpsDbCommand : DbCommand
{
    /// <inheritdoc/>
    public override string CommandText { get; set; } = string.Empty;

    /// <inheritdoc/>
    public override int CommandTimeout { get; set; }

    /// <inheritdoc/>
    public override CommandType CommandType { get; set; }

    /// <inheritdoc/>
    public override bool DesignTimeVisible { get; set; }

    /// <inheritdoc/>
    public override UpdateRowSource UpdatedRowSource { get; set; }

    /// <inheritdoc/>
    protected override DbConnection? DbConnection { get; set; }

    /// <inheritdoc/>
    protected override DbParameterCollection DbParameterCollection =>
        throw new NotSupportedException("DbParameters are not supported.");

    /// <inheritdoc/>
    protected override DbTransaction? DbTransaction { get; set; }

    /// <inheritdoc/>
    public override void Cancel()
    {
        throw new NotSupportedException("Cancellation is not supported.");
    }

    /// <inheritdoc/>
    public override int ExecuteNonQuery()
    {
        throw new NotSupportedException("Non-query commands are not supported.");
    }

    /// <inheritdoc/>
    public override object? ExecuteScalar()
    {
        using var reader = ExecuteDbDataReader(CommandBehavior.Default);

        return reader.GetValue(0);
    }

    /// <inheritdoc/>
    public override void Prepare()
    {
        throw new NotSupportedException("Prepared statements are not supported.");
    }

    /// <inheritdoc/>
    protected override DbParameter CreateDbParameter()
    {
        throw new NotSupportedException("DbParameters are not supported.");
    }

    /// <inheritdoc/>
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

        var reader = new TpsDataReader(
            tpsFile: tpsFile,
            tableDefinition: tableDef,
            tableNumber: tableNumber,
            fieldIteratorNodes: FieldValueReader.CreateFieldIteratorNodes(
                fieldDefinitions: tableDef.Fields,
                requestedFieldIndexes: [.. tableDef.Fields.Select(f => f.Index)]));
        
        reader.NextResult();

        return reader;
    }
}