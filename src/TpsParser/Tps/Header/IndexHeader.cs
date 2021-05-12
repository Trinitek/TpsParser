using System;

namespace TpsParser.Tps.Header
{
    public sealed class IndexHeader : HeaderBase
    {
        public byte IndexNumber => (byte)Kind;

        /// <summary>
        /// Instantiates a new index header.
        /// </summary>
        /// <param name="tableNumber"></param>
        /// <param name="kind"></param>
        public IndexHeader(int tableNumber, HeaderKind kind)
            : base(tableNumber, kind)
        { }

        /// <inheritdoc/>
        public override string ToString() =>
            $"IndexHeader({IndexNumber})";

        /// <summary>
        /// Creates a new <see cref="IndexHeader"/> from the given reader.
        /// </summary>
        /// <param name="rx"></param>
        /// <returns></returns>
        public static IndexHeader Read(TpsReader rx)
        {
            if (rx is null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            return new IndexHeader(
                tableNumber: rx.ReadLongBE(),
                kind: (HeaderKind)rx.ReadByte());
        }
    }
}
