using System;

namespace TpsParser.Tps.Header
{
    /// <summary>
    /// Encapsulates metadata information about particular record.
    /// </summary>
    public sealed class MetadataHeader : HeaderBase
    {
        /// <summary>
        /// Gets the data type that this header describes.
        /// </summary>
        public byte DescribesType { get; }

        /// <summary>
        /// True if this header describes a data record.
        /// </summary>
        public bool DescribesData => DescribesType == (byte)HeaderKind.Data;

        /// <summary>
        /// True if this header describes a key or index.
        /// </summary>
        public bool DescribesKeyOrIndex => DescribesType < (byte)HeaderKind.Data;

        /// <summary>
        /// Instantiates a new header.
        /// </summary>
        /// <param name="tableNumber"></param>
        /// <param name="kind"></param>
        /// <param name="describesType"></param>
        public MetadataHeader(int tableNumber, HeaderKind kind, byte describesType)
            : base(tableNumber, kind)
        {
            AssertIsType(HeaderKind.Metadata);

            DescribesType = describesType;
        }

        /// <inheritdoc/>
        public override string ToString() =>
            $"IndexHeader({(DescribesData ? "Data" : DescribesKeyOrIndex ? $"Index({DescribesType})" : "??")})";

        /// <summary>
        /// Creates a new <see cref="MetadataHeader"/> from the given reader.
        /// </summary>
        /// <param name="rx"></param>
        /// <returns></returns>
        public static MetadataHeader Read(TpsReader rx)
        {
            if (rx is null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            return new MetadataHeader(
                tableNumber: rx.ReadLongBE(),
                kind: (HeaderKind)rx.ReadByte(),
                describesType: rx.ReadByte());
        }
    }
}
