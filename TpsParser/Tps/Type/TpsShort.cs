using System;
using TpsParser.Binary;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a signed short.
    /// </summary>
    public sealed class TpsShort : TpsObject<short>
    {
        public override string TypeName => "SHORT";

        public override int TypeCode => 0x02;

        public TpsShort(RandomAccess rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Value = rx.ShortLE();
        }
    }
}
