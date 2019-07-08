using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TpsParser.Tps.Header;
using TpsParser.Tps.Type;

namespace TpsParser.Tps.Record
{
    public sealed class DataRecord
    {
        private DataHeader Header { get; }
        public TableDefinitionRecord TableDefinition { get; }
        public IEnumerable<TpsObject> Values { get; }
        public TpsRecord Record { get; }

        public int RecordNumber => Header.RecordNumber;

        public DataRecord(TpsRecord tpsRecord, TableDefinitionRecord tableDefinition)
        {
            Record = tpsRecord ?? throw new ArgumentNullException(nameof(tpsRecord));
            TableDefinition = tableDefinition ?? throw new ArgumentNullException(nameof(tableDefinition));
            Header = (DataHeader)Record.Header;
            Values = TableDefinition.Parse(tpsRecord.Data.GetRemainder());
        }

        public IReadOnlyDictionary<string, TpsObject> GetFieldValuePairs() =>
            TableDefinition.Fields
                .Zip(Values, (field, value) => (field, value))
                .ToDictionary(pair => pair.field.Name, pair => pair.value);

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
