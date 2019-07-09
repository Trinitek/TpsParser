using System;
using TpsParser.Binary;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents an unsigned integer.
    /// </summary>
    public sealed class TpsUnsignedLong : TpsObject<uint>
    {
        public override TpsTypeCode TypeCode => TpsTypeCode.ULong;

        public TpsUnsignedLong(RandomAccess rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Value = rx.UnsignedLongLE();
        }
    }
}
