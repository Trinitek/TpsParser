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

        /// <summary>
        /// Instantiates a new SREAL.
        /// </summary>
        /// <param name="rx"></param>
        public TpsFloat(RandomAccess rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Value = rx.FloatLE();
        }

        /// <summary>
        /// Returns true if the value is not zero.
        /// </summary>
        protected override bool AsBoolean() => Value != 0.0;
    }
}
