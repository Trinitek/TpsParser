using System;
using TpsParser.Binary;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a byte.
    /// </summary>
    public sealed class TpsByte : TpsObject<byte>
    {
        public override TpsTypeCode TypeCode => TpsTypeCode.Byte;

        public TpsByte(RandomAccess rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Value = rx.Byte();
        }
    }
}
