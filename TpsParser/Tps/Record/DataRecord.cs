using System.Collections.Generic;
using System.Text;
using TpsParser.Tps.Header;

namespace TpsParser.Tps.Record
{
    public sealed class DataRecord
    {
        private DataHeader Header { get; }
        public TableDefinitionRecord TableDefinition { get; }
        public IEnumerable<object> Values { get; }
        public TpsRecord Record { get; }

        public int RecordNumber => Header.RecordNumber;

        public DataRecord(TpsRecord tpsRecord, TableDefinitionRecord tableDefinition)
        {
            Record = tpsRecord ?? throw new System.ArgumentNullException(nameof(tpsRecord));
            TableDefinition = tableDefinition ?? throw new System.ArgumentNullException(nameof(tableDefinition));
            Header = (DataHeader)Record.Header;
            Values = TableDefinition.Parse(tpsRecord.Data.GetRemainder());
        }

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
