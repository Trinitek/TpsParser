using System.Globalization;

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
        /// <param name="value"></param>
        public TpsFloat(float value)
        {
            Value = value;
        }

        /// <summary>
        /// Returns true if the value is not zero.
        /// </summary>
        public override Maybe<bool> ToBoolean() => new Maybe<bool>(Value != 0.0);

        /// <inheritdoc/>
        public override Maybe<float> ToFloat() => new Maybe<float>(Value);

        /// <inheritdoc/>
        public override Maybe<double> ToDouble() => new Maybe<double>(Value);

        /// <inheritdoc/>
        public override Maybe<decimal> ToDecimal() => new Maybe<decimal>((decimal)Value);

        /// <inheritdoc/>
        public override string ToString(string format) => Value.ToString(format, CultureInfo.InvariantCulture);
    }
}
