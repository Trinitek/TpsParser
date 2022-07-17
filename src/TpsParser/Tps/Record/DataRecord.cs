using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TpsParser.Tps.Header;
using TpsParser.Tps.Type;

namespace TpsParser.Tps.Record
{
    /// <summary>
    /// Encapsulates field values that belong to a particular row.
    /// </summary>
    public interface IDataRecord
    {
        /// <summary>
        /// Gets the table definition for the table that owns the record.
        /// </summary>
        ITableDefinitionRecord TableDefinition { get; }

        /// <summary>
        /// Gets the values for the record. The order of the values matches the order of <see cref="ITableDefinitionRecord.Fields"/>.
        /// </summary>
        IReadOnlyList<ITpsObject> Values { get; }

        /// <summary>
        /// Gets the low level representation of the record in the file.
        /// </summary>
        TpsRecord Record { get; }

        /// <summary>
        /// Gets the record number.
        /// </summary>
        uint RecordNumber { get; }

        /// <summary>
        /// Gets a dictionary of field names and their associated values.
        /// </summary>
        /// <returns></returns>
        IReadOnlyDictionary<string, ITpsObject> GetFieldValuePairs();
    }

    internal sealed class DataRecord : IDataRecord
    {
        private DataHeader Header { get; }

        public ITableDefinitionRecord TableDefinition { get; }

        public IReadOnlyList<ITpsObject> Values { get; }

        public TpsRecord Record { get; }

        public uint RecordNumber => Header.RecordNumber;

        /// <summary>
        /// Instantiates a new data record.
        /// </summary>
        /// <param name="tpsRecord">The underlying record that contains the low-level file information.</param>
        /// <param name="tableDefinition">The table definition for the table to which the record belongs.</param>
        public DataRecord(TpsRecord tpsRecord, ITableDefinitionRecord tableDefinition)
        {
            Record = tpsRecord ?? throw new ArgumentNullException(nameof(tpsRecord));
            TableDefinition = tableDefinition ?? throw new ArgumentNullException(nameof(tableDefinition));
            Header = (DataHeader)Record.Header;
            Values = TableDefinition.Parse(tpsRecord.Data.GetRemainderAsByteArray());
        }

        public IReadOnlyDictionary<string, ITpsObject> GetFieldValuePairs() =>
            TableDefinition.Fields
                .Where(f => f.OwnerGroup is null)
                .Zip(Values, (field, value) => (field, value))
                .ToDictionary(pair => pair.field.Name, pair => pair.value);

        /// <inheritdoc/>
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append($"{RecordNumber} :");

            foreach (var value in Values)
            {
                sb.Append($" {value}");
            }

            return sb.ToString();
        }
    }
}
