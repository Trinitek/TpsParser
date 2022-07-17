using System;

namespace TpsParser.Tps.Header
{
    /// <summary>
    /// Encapsulates information about a particular <see cref="TpsRecord"/>.
    /// </summary>
    public sealed class DataHeader : HeaderBase
    {
        /// <summary>
        /// Gets the record number of the data entry this header describes.
        /// </summary>
        public uint RecordNumber { get; }

        /// <summary>
        /// Instantiates a new header.
        /// </summary>
        /// <param name="tableNumber"></param>
        /// <param name="kind"></param>
        /// <param name="recordNumber"></param>
        public DataHeader(int tableNumber, HeaderKind kind, uint recordNumber)
            : base(tableNumber, kind)
        {
            AssertIsType(HeaderKind.Data);

            RecordNumber = recordNumber;
        }

        /// <inheritdoc/>
        public override string ToString() =>
            $"DataHeader({TableNumber}, {RecordNumber})";

        /// <summary>
        /// Creates a new <see cref="DataHeader"/> from the given reader.
        /// </summary>
        /// <param name="rx"></param>
        /// <returns></returns>
        public static DataHeader Read(TpsReader rx)
        {
            if (rx is null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            return new DataHeader(
                tableNumber: rx.ReadLongBE(),
                kind: (HeaderKind)rx.ReadByte(),
                recordNumber: rx.ReadUnsignedLongBE());
        }
    }
}
