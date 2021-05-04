using System;
using System.Collections.Generic;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a value that may or may not be present.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct Maybe<T> : IEquatable<Maybe<T>>
    {
        /// <summary>
        /// Returns true if a value is present and valid.
        /// </summary>
        public bool HasValue { get; }

        /// <summary>
        /// Gets the value if available.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no value has been assigned.
        /// </exception>
        public T Value => HasValue ? _value : throw new InvalidOperationException("No value has been assigned.");
        private readonly T _value;

        /// <summary>
        /// Instantiates a new instance with a value.
        /// </summary>
        /// <param name="value">The value to use. Must not be null.</param>
        public Maybe(T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            HasValue = true;
            _value = value;
        }

        /// <inheritdoc/>
        public bool Equals(Maybe<T> other) =>
            HasValue == other.HasValue
            && (!HasValue || _value.Equals(other._value));

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is Maybe<T> m && Equals(m);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -1906564177;
            hashCode = hashCode * -1521134295 + HasValue.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(_value);
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(Maybe<T> left, Maybe<T> right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(Maybe<T> left, Maybe<T> right) => !(left == right);
    }
}
