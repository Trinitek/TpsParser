using System;
using System.Collections.Generic;
using TpsParser.Binary;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a blob of bytes.
    /// </summary>
    public sealed class TpsBlob : TpsObject<IEnumerable<byte>>
    {
        public override TpsTypeCode TypeCode => TpsTypeCode.Blob;

        public TpsBlob(RandomAccess rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Value = rx.ReadBytes(rx.LongLE());
        }
    }
}
