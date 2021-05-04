using System;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a Pascal string where the length is specified at the beginning of the string.
    /// </summary>
    public sealed class TpsPString : TpsObject<string>
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.PString;

        /// <summary>
        /// Instantiate a new PSTRING.
        /// </summary>
        /// <param name="value">The string value. Must not be null.</param>
        public TpsPString(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Returns true if the string length is not zero.
        /// </summary>
        public override Maybe<bool> ToBoolean() => new Maybe<bool>(Value.Length > 0);
    }
}
