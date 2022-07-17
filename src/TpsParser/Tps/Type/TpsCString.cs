using System;
using System.Collections.Generic;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a null-terminated string.
    /// </summary>
    public readonly struct TpsCString : IString, IEquatable<TpsCString>
    {
        /// <inheritdoc/>
        public TpsTypeCode TypeCode => TpsTypeCode.CString;

        /// <summary>
        /// Gets the string backing this type.
        /// </summary>
        public string Value { get; }

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
        public bool ToBoolean() => Value.Length > 0 && Value.Trim(' ').Length > 0;


        /// <inheritdoc/>
        public override string ToString() => Value;

        /// <inheritdoc/>
        public bool Equals(TpsCString other) =>
            Value == other.Value;

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is TpsCString x && Equals(x);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return -1937169414 + EqualityComparer<string>.Default.GetHashCode(Value);
        }

        /// <inheritdoc/>
        public static bool operator ==(TpsCString left, TpsCString right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(TpsCString left, TpsCString right) => !(left == right);
    }
}
