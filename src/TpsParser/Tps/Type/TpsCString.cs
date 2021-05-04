using System;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a null-terminated string.
    /// </summary>
    public sealed class TpsCString : TpsObject<string>
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.CString;

        /// <summary>
        /// Instantiates a new CSTRING.
        /// </summary>
        /// <param name="value">The string value. Must not be null.</param>
        public TpsCString(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Returns true if the string's length is greater than zero.
        /// </summary>
        public override Maybe<bool> ToBoolean() => new Maybe<bool>(Value.Length > 0);
    }
}
