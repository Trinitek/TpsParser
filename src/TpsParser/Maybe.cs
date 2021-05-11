using System;
using System.Collections.Generic;

namespace TpsParser
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
        public T Value => HasValue ? _value : throw new InvalidOperationException(
                $"The TPS object does not implement a conversion to {typeof(T)}. No value has been assigned.");
        private readonly T _value;

        /// <summary>
        /// Gets the value or the default value of T if it is not set.
        /// </summary>
        public T ValueOrDefault => HasValue ? Value : default;

        //public TOther ValueOr<TOther>() => HasValue ? (TOther)Value : default(TOther);

        /// <summary>
        /// Instantiates a new instance with a value.
        /// </summary>
        /// <param name="value">The value to use. Must not be null.</param>
        public Maybe(T value)
        {
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

    internal static class MaybeExtensions
    {
        public static T? AsNullable<T>(this Maybe<T> maybe) where T : struct
            => maybe.HasValue ? maybe.Value : default(T?);
    }
}
