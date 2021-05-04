using System;
using System.Collections.Generic;
using System.Linq;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a blob of bytes.
    /// </summary>
    public sealed class TpsBlob : TpsObject<IEnumerable<byte>>
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

            Value = rx.ReadBytes(rx.LongLE());
        }

        /// <summary>
        /// Returns true if the size of the blob is not zero.
        /// </summary>
        internal override bool AsBoolean() => Value.Any();
    }
}
