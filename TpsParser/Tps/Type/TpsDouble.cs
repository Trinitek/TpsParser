using System;
using TpsParser.Binary;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a double-precision floating point number.
    /// </summary>
    public sealed class TpsDouble : TpsObject<double>
    {
        public override TpsTypeCode TypeCode => TpsTypeCode.BFloat8;

        public TpsDouble(RandomAccess rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Value = rx.DoubleLE();
        }
    }
}
