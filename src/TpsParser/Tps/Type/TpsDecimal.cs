using System;
using System.Linq;
using TpsParser.Binary;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a binary coded decimal.
    /// </summary>
    public sealed class TpsDecimal : TpsObject<string>
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.Decimal;

        /// <summary>
        /// Gets the value as a <see cref="decimal"/>. Clarion allows values up to 31 figures which exceeds <see cref="decimal"/>'s 29, so precision loss is possible.
        /// </summary>
        public decimal ValueAsDecimal { get; }

        /// <summary>
        /// Instantiates a new DECIMAL.
        /// </summary>
        /// <param name="rx">The binary reader that contains the raw data.</param>
        /// <param name="length">The total number of digits in the value.</param>
        /// <param name="digitsAfterDecimal">The number of digits that follow after the decimal point.</param>
        public TpsDecimal(RandomAccess rx, int length, int digitsAfterDecimal)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Value = rx.BinaryCodedDecimal(length, digitsAfterDecimal);

            decimal.TryParse(Value, out decimal decValue);

            ValueAsDecimal = decValue;
        }

        /// <summary>
        /// Returns true if the value is not zero.
        /// </summary>
        /// <returns></returns>
        public override bool AsBoolean() => !Value.Any(v => v == '0' || v == '.');
    }
}
