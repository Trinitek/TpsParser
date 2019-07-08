using System;
using TpsParser.Binary;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents an unsigned integer.
    /// </summary>
    public sealed class TpsUnsignedLong : TpsObject<uint>
    {
        public override string TypeName => "ULONG";

        public override int TypeCode => 0x07;

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
