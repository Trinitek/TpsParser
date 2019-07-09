using System;
using System.Collections.Generic;
using System.Text;
using TpsParser.Binary;
using TpsParser.Tps.Header;
using TpsParser.Tps.Type;

namespace TpsParser.Tps.Record
{
    public sealed class MemoRecord
    {
        private MemoHeader Header { get; }
        private RandomAccess Data { get; }

        /// <summary>
        /// Gets the number of the <see cref="DataRecord"/> that owns this memo.
        /// </summary>
        public int Owner => Header.OwningRecord;

        public MemoRecord(MemoHeader header, RandomAccess rx)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));
            Data = rx ?? throw new ArgumentNullException(nameof(rx));
        }

        /// <summary>
        /// Returns the memo data as an ISO-8859-1 encoded string.
        /// </summary>
        /// <returns></returns>
        public string GetDataAsMemo() =>
            Encoding.GetEncoding("ISO-8859-1").GetString(Data.GetData());

        /// <summary>
        /// Returns the memo data as a raw byte array.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<byte> GetDataAsBlob() =>
            Data.ReadBytes(Data.LongLE());

        public TpsObject GetValue(MemoDefinitionRecord memoDefinitionRecord)
        {
            if (memoDefinitionRecord == null)
            {
                throw new ArgumentNullException(nameof(memoDefinitionRecord));
            }

            if (memoDefinitionRecord.IsBlob)
            {
                return new TpsBlob(Data);
            }
            else
            {
                return new TpsString(Data, Encoding.GetEncoding("ISO-8859-1"));
            }
        }
    }
}
