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
                $"The TPS object value is not representable as a {typeof(T)}.");
        private readonly T _value;

        /// <summary>
        /// Instantiates a new instance with a value.
        /// </summary>
        /// <param name="value"></param>
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

    /// <summary>
    /// Static extension methods for working with <see cref="Maybe{T}"/>.
    /// </summary>
    public static class Maybe
    {
        /// <summary>
        /// Creates a <see cref="Maybe{T}"/> that has no value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Maybe<T> None<T>() => new Maybe<T>();

        /// <summary>
        /// Creates a <see cref="Maybe{T}"/> with the given value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Maybe<T> Some<T>(T value) => new Maybe<T>(value);

        /// <summary>
        /// Converts a <see cref="Maybe{T}"/> into a <see cref="Nullable{T}"/> that is null if no value is assigned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="maybe"></param>
        /// <returns></returns>
        public static T? AsNullable<T>(this Maybe<T> maybe) where T : struct
            => maybe.HasValue ? maybe.Value : default(T?);

        /// <summary>
        /// Changes the result type of a <see cref="Maybe{T}"/> if a value is present.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="maybe"></param>
        /// <param name="conversion"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Maybe<TResult> Convert<TSource, TResult>(this Maybe<TSource> maybe, Func<TSource, Maybe<TResult>> conversion)
        {
            if (conversion is null)
            {
                throw new ArgumentNullException(nameof(conversion));
            }

            return maybe.HasValue
                ? conversion.Invoke(maybe.Value)
                : Maybe.None<TResult>();
        }

        /// <summary>
        /// Changes the result type of a <see cref="Maybe{T}"/> if a value is present.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="maybe"></param>
        /// <param name="conversion"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Maybe<TResult> ConvertSome<TSource, TResult>(this Maybe<TSource> maybe, Func<TSource, TResult> conversion)
        {
            if (conversion is null)
            {
                throw new ArgumentNullException(nameof(conversion));
            }

            return maybe.HasValue
                ? Maybe.Some(conversion.Invoke(maybe.Value))
                : Maybe.None<TResult>();
        }
    }
}
