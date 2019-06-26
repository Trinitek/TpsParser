using System;
using System.Collections.Generic;
using System.Text;
using TpsParser.Binary;
using TpsParser.Tps.Header;

namespace TpsParser.Tps.Record
{
    public sealed class MemoRecord
    {
        private MemoHeader Header { get; }
        private RandomAccess Data { get; }

        public int Owner => Header.OwningRecord;

        public MemoRecord(MemoHeader header, RandomAccess rx)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));
            Data = rx ?? throw new ArgumentNullException(nameof(rx));
        }

        public string GetDataAsMemo() =>
            Encoding.GetEncoding("ISO-8859-1").GetString(Data.GetData());

        public IEnumerable<byte> GetDataAsBlob() =>
            Data.ReadBytes(Data.LongLE());
    }
}
