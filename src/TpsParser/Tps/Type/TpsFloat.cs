using System;
using TpsParser.Binary;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a single-precision floating point number.
    /// </summary>
    public sealed class TpsFloat : TpsObject<float>
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.SReal;

        public TpsFloat(RandomAccess rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Value = rx.FloatLE();
        }
    }
}
