using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TpsParser.TypeModel;

namespace TpsParser.Tps.Record;

/// <summary>
/// Encapsulates field values that belong to a particular row.
/// </summary>
public interface IDataRecord
{
    /// <summary>
    /// Gets the table definition for the table that owns the record.
    /// </summary>
    TableDefinition TableDefinition { get; }

    /// <summary>
    /// Gets the values for the record. The order of the values matches the order of <see cref="TableDefinition.Fields"/>.
    /// </summary>
    IReadOnlyList<IClaObject> Values { get; }

    /// <summary>
    /// Gets the low level representation of the record in the file.
    /// </summary>
    TpsRecord Record { get; }

    /// <summary>
    /// Gets the record number.
    /// </summary>
    int RecordNumber { get; }

    /// <summary>
    /// Gets a dictionary of field names and their associated values.
    /// </summary>
    /// <returns></returns>
    IReadOnlyDictionary<string, IClaObject> GetFieldValuePairs();
}

/// <inheritdoc/>
internal sealed class DataRecord : IDataRecord
{
    /// <inheritdoc/>
    public TableDefinition TableDefinition { get; }

    /// <inheritdoc/>
    public IReadOnlyList<IClaObject> Values { get; }

    /// <inheritdoc/>
    public TpsRecord Record { get; }

    public DataRecordPayload DataRecordPayload => (DataRecordPayload)Record.GetPayload()!;

    /// <inheritdoc/>
    public int RecordNumber => DataRecordPayload.RecordNumber;

    /// <summary>
    /// Instantiates a new data record.
    /// </summary>
    /// <param name="tpsRecord">The underlying record that contains the low-level file information.</param>
    /// <param name="tableDefinition">The table definition for the table to which the record belongs.</param>
    /// <param name="encoding"></param>
    public DataRecord(TpsRecord tpsRecord, TableDefinition tableDefinition, Encoding encoding)
    {
        Record = tpsRecord ?? throw new ArgumentNullException(nameof(tpsRecord));
        TableDefinition = tableDefinition ?? throw new ArgumentNullException(nameof(tableDefinition));

        var rx = new TpsRandomAccess(DataRecordPayload.Content.ToArray(), encoding);

        Values = TableDefinition.ParseFields(rx);
    }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, IClaObject> GetFieldValuePairs() =>
        TableDefinition.Fields
            .Zip(Values, (field, value) => (field, value))
            .ToDictionary(pair => pair.field.Name, pair => pair.value);
}
