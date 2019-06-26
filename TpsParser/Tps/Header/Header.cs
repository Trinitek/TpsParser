using System;
using TpsParser.Binary;

namespace TpsParser.Tps.Header
{
    public abstract class Header
    {
        public int TableNumber { get; }

        protected int TableType { get; }

        public Header(RandomAccess rx)
            : this(rx, true)
        { }

        public Header(RandomAccess rx, bool readTable)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            if (readTable)
            {
                TableNumber = rx.LongBE();
            }

            TableType = rx.Byte();
        }

        protected void AssertIsType(int expected)
        {
            if (TableType != expected)
            {
                throw new ArgumentException($"Header is not of expected type. Expected {expected} but was {TableType}.");
            }
        }
    }
}
