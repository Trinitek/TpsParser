using System;
using TpsParser.Tps.Header;

namespace TpsParser.Tps.Record
{
    public sealed class IndexRecord
    {
        private IndexHeader Header { get; }
        public int RecordNumber { get; }

        public IndexRecord(TpsRecord tpsRecord)
        {
            if (tpsRecord == null)
            {
                throw new ArgumentNullException(nameof(tpsRecord));
            }

            Header = (IndexHeader)tpsRecord.Header;

            var data = tpsRecord.Data;
            data.JumpAbsolute(data.Length - 4);

            RecordNumber = data.LongBE();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override string ToString() =>
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
            $"IndexRecord({Header.TableNumber},{Header.IndexNumber},{RecordNumber})";
    }
}
