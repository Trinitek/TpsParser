using System;
using TpsParser.Tps.Header;

namespace TpsParser.Tps.Record
{
    public sealed class TableNameRecord
    {
        public TableNameHeader Header { get; }
        public int TableNumber { get; }

        public TableNameRecord(TpsRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            Header = (TableNameHeader)record.Header;
            TableNumber = record.Data.LongBE();
        }

        public override string ToString()
        {
            return $"TableRecord({Header.Name},{TableNumber})";
        }
    }
}
