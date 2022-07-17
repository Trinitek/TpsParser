using System;
using System.Collections.Generic;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a fixed length string.
    /// </summary>
    public readonly struct TpsString : IString, IEquatable<TpsString>
    {
        /// <inheritdoc/>
        public TpsTypeCode TypeCode => TpsTypeCode.String;

        /// <summary>
        /// Gets the string backing this type.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Instantiates a new STRING.
        /// </summary>
        /// <param name="value">The string value. Must not be null.</param>
        public TpsString(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc/>
        public bool ToBoolean() => Value.Length > 0 && Value.Trim(' ').Length > 0;

        public override string ToString() => Value;

        /// <inheritdoc/>
        public bool Equals(TpsString other) =>
            Value == other.Value;

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is TpsString x && Equals(x);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return -1937169414 + EqualityComparer<string>.Default.GetHashCode(Value);
        }

        /// <inheritdoc/>
        public static bool operator ==(TpsString left, TpsString right) => EqualityComparer<TpsString>.Default.Equals(left, right);

        /// <inheritdoc/>
        public static bool operator !=(TpsString left, TpsString right) => !(left == right);
    }
}
