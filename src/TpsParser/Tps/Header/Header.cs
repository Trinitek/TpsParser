using System;
using TpsParser.Binary;

namespace TpsParser.Tps.Header
{
    /// <summary>
    /// Encapsulates information that describes a record.
    /// </summary>
    public interface IHeader
    {
        /// <summary>
        /// Gets the table number to which the header belongs.
        /// </summary>
        int TableNumber { get; }
    }

    /// <inheritdoc/>
    public abstract class Header : IHeader
    {
        /// <inheritdoc/>
        public int TableNumber { get; }

        /// <summary>
        /// Gets the type code that represents the type of table.
        /// </summary>
        protected int TableType { get; }

        /// <summary>
        /// Instantiates a new header.
        /// </summary>
        /// <param name="rx"></param>
        public Header(TpsReader rx)
            : this(rx, true)
        { }

        /// <summary>
        /// Instantiates a new header.
        /// </summary>
        /// <param name="rx"></param>
        /// <param name="readTable"></param>
        public Header(TpsReader rx, bool readTable)
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
