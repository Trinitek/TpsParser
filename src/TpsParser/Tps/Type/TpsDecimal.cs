using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a binary coded decimal.
    /// </summary>
    public sealed class TpsDecimal : TpsObject<string>
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.Decimal;

        private decimal _valueAsDecimal;

        /// <summary>
        /// Instantiates a new DECIMAL.
        /// </summary>
        /// <param name="value">
        /// <para>
        /// The string representation of the decimal.
        /// </para>
        /// <para>
        /// This value must have at least one digit before the decimal point. A leading negation sign is permitted.
        /// The decimal point is optional for whole numbers. It must not be blank or null. Spaces or characters that
        /// are not digits, '.', or '-' are not permitted.
        /// </para>
        /// </param>
        public TpsDecimal(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));

            if (!Regex.IsMatch(value, @"^-?\d+\.?\d*$"))
            {
                throw new ArgumentException("The given value does not represent a well-formed number.", nameof(value));
            }

            _ = decimal.TryParse(Value, out decimal decValue);

            _valueAsDecimal = decValue;
        }

        /// <summary>
        /// Returns true if the value is not zero.
        /// </summary>
        public override Maybe<bool> ToBoolean() => new Maybe<bool>(Value.Where(v => char.IsDigit(v)).Any(v => v != '0'));

        /// <summary>
        /// Gets the value as a <see cref="decimal"/>. Clarion allows values up to 31 figures which exceeds <see cref="decimal"/>'s 29, so precision loss is possible.
        /// </summary>
        public override Maybe<decimal> ToDecimal() => new Maybe<decimal>(_valueAsDecimal);

        /// <summary>
        /// Gets a <see cref="DateTime"/> by treating the value as a Clarion Standard Date, where the value is the number of days since <see cref="TpsDate.ClarionEpoch"/>.
        /// For more information about the Clarion Standard Date, see the remarks section of <see cref="TpsDate"/>.
        /// </summary>
        public override Maybe<DateTime?> ToDateTime() => new Maybe<DateTime?>(TpsDate.ClarionEpoch.AddDays((double)_valueAsDecimal));
    }
}
