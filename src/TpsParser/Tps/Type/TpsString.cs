using System;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a fixed length string.
    /// </summary>
    public sealed class TpsString : TpsObject<string>
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.String;

        /// <summary>
        /// Instantiates a new STRING.
        /// </summary>
        /// <param name="value">The string value. Must not be null.</param>
        public TpsString(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Returns true if the string contains more than only whitespace.
        /// </summary>
        public override Maybe<bool> ToBoolean() => new Maybe<bool>(!string.IsNullOrWhiteSpace(Value));
    }
}
