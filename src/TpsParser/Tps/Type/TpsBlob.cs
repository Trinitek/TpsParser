using System;
using System.Linq;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents an arbitrary array of bytes. Unlike <see cref="TpsMemo"/>,
    /// a BLOB can be larger than 65,536 bytes.
    /// </summary>
    public sealed class TpsBlob : TpsObject<byte[]>
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.None;

        /// <summary>
        /// Instantiates a new BLOB.
        /// </summary>
        /// <param name="rx"></param>
        public TpsBlob(TpsReader rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Value = rx.ReadBytes(rx.ReadLongLE());
        }

        /// <summary>
        /// Returns true if the size of the blob is not zero.
        /// </summary>
        public override Maybe<bool> ToBoolean() => new Maybe<bool>(Value.Any());
    }
}
