using System;
using TpsParser.Binary;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a signed integer.
    /// </summary>
    public sealed class TpsLong : TpsObject<int>
    {
        public override TpsTypeCode TypeCode => TpsTypeCode.Long;

        public TpsLong(RandomAccess rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Value = rx.LongLE();
        }
    }
}
