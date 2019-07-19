using System;
using System.Collections.Generic;
using System.Linq;
using TpsParser.Binary;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a blob of bytes.
    /// </summary>
    public sealed class TpsBlob : TpsObject<IEnumerable<byte>>
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.Blob;

        /// <summary>
        /// Instantiates a new BLOB.
        /// </summary>
        /// <param name="rx"></param>
        public TpsBlob(RandomAccess rx)
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
        /// <returns></returns>
        public override bool AsBoolean() => Value.Count() > 0;
    }
}
