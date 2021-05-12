using System.Globalization;

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
        /// <param name="value"></param>
        public TpsDouble(double value)
        {
            Value = value;
        }

        /// <summary>
        /// Returns true if the value is not zero.
        /// </summary>
        public override Maybe<bool> ToBoolean() => new Maybe<bool>(Value != 0.0);

        /// <inheritdoc/>
        public override Maybe<double> ToDouble() => new Maybe<double>(Value);

        /// <inheritdoc/>
        public override Maybe<decimal> ToDecimal() => new Maybe<decimal>((decimal)Value);

        /// <inheritdoc/>
        public override string ToString(string format) => Value.ToString(format, CultureInfo.InvariantCulture);
    }
}
