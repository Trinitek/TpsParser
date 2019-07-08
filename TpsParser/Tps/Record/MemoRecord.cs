﻿using System;
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

        /// <summary>
        /// Gets the ID of the <see cref="DataRecord"/> that owns this record.
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
    }
}
