using System;
using TpsParser.Binary;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents an unsigned short.
    /// </summary>
    public sealed class TpsUnsignedShort : TpsObject<ushort>
    {
        public override TpsTypeCode TypeCode => TpsTypeCode.UShort;

        public TpsUnsignedShort(RandomAccess rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Value = rx.UnsignedShortLE();
        }
    }
}
