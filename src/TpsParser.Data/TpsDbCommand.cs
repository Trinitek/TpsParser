using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace TpsParser.Data;

/// <summary>
/// Represents a SQL statement to execute against a <see cref="TpsDbConnection"/>.
/// </summary>
public partial class TpsDbCommand : DbCommand
{
    /// <inheritdoc/>
    [AllowNull]
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

    /// <summary>
    /// Instantiates a new <see cref="TpsDbCommand"/>.
    /// </summary>
    public TpsDbCommand()
    { }

    /// <summary>
    /// Instantiates a new <see cref="TpsDbCommand"/>.
    /// </summary>
    /// <param name="commandText"></param>
    public TpsDbCommand(string? commandText)
    {
        CommandText = commandText;
    }

    /// <summary>
    /// Instantiates a new <see cref="TpsDbCommand"/>.
    /// </summary>
    /// <param name="commandText"></param>
    /// <param name="connection"></param>
    public TpsDbCommand(string? commandText, TpsDbConnection? connection)
    {
        CommandText = commandText;
        DbConnection = connection;
    }

    private TpsDbConnection AssertConnectionIsOpen()
    {
        if (DbConnection is null)
        {
            throw new InvalidOperationException("DbConnection was not specified.");
        }

        if (DbConnection.State != ConnectionState.Open)
        {
            throw new InvalidOperationException("DbConnection is not open.");
        }

        return (TpsDbConnection)DbConnection;
    }

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
        var conn = AssertConnectionIsOpen();

        (string baseFileName, string? tableName) = QueryParser.GetFileTableName(CommandText);

        var tpsFileConnectionContext = conn.GetOrOpenTpsFile(requestedFileName: baseFileName);

        var tpsFile = tpsFileConnectionContext.TpsFile;

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
            connectionContext: tpsFileConnectionContext,
            tableDefinition: tableDef,
            tableNumber: tableNumber,
            fieldIteratorNodes: FieldValueReader.CreateFieldIteratorNodes(
                fieldDefinitions: tableDef.Fields,
                requestedFieldIndexes: [.. tableDef.Fields.Select(f => f.Index)]));
        
        reader.NextResult();

        return reader;
    }
}