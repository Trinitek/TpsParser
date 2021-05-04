using System;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a double-precision floating point number.
    /// </summary>
    public sealed class TpsDouble : TpsObject<double>
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.Real;

        /// <summary>
        /// Instantiates a new REAL.
        /// </summary>
        /// <param name="rx"></param>
        public TpsDouble(TpsReader rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Value = rx.DoubleLE();
        }

        /// <summary>
        /// Returns true if the value is not zero.
        /// </summary>
        internal override bool AsBoolean() => Value != 0.0;
    }
}
