using System;
using System.Collections.Generic;
using System.Globalization;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a signed short.
    /// </summary>
    public readonly struct TpsShort : ISimple, IEquatable<TpsShort>
    {
        /// <inheritdoc/>
        public TpsTypeCode TypeCode => TpsTypeCode.Short;

        private short Value { get; }

        /// <summary>
        /// Instantiates a new SHORT.
        /// </summary>
        /// <param name="value"></param>
        public TpsShort(short value) => Value = value;

        /// <summary>
        /// Returns true if the value is not zero.
        /// </summary>
        public Maybe<bool> ToBoolean() => Maybe.Some(Value != 0);

        /// <inheritdoc/>
        public Maybe<ushort> ToUInt16() =>
            Value < 0
            ? Maybe.None<ushort>()
            : Maybe.Some((ushort)Value);

        /// <inheritdoc/>
        public Maybe<short> ToInt16() => Maybe.Some(Value);

        /// <inheritdoc/>
        public Maybe<uint> ToUInt32() =>
            Value < 0
            ? Maybe.None<uint>()
            : Maybe.Some((uint)Value);

        /// <inheritdoc/>
        public Maybe<int> ToInt32() => Maybe.Some<int>(Value);

        /// <inheritdoc/>
        public Maybe<ulong> ToUInt64() =>
            Value < 0
            ? Maybe.None<ulong>()
            : Maybe.Some((ulong)Value);

        /// <inheritdoc/>
        public Maybe<long> ToInt64() => Maybe.Some<long>(Value);

        /// <inheritdoc/>
        public Maybe<decimal> ToDecimal() => Maybe.Some<decimal>(Value);

        /// <inheritdoc/>
        public Maybe<sbyte> ToSByte() =>
            sbyte.MinValue > Value || sbyte.MaxValue < Value
            ? Maybe.None<sbyte>()
            : Maybe.Some((sbyte)Value);

        /// <inheritdoc/>
        public Maybe<byte> ToByte() =>
            byte.MinValue > Value || byte.MaxValue < Value
            ? Maybe.None<byte>()
            : Maybe.Some((byte)Value);

        /// <inheritdoc/>
        public Maybe<float> ToFloat() => Maybe.Some<float>(Value);

        /// <inheritdoc/>
        public Maybe<double> ToDouble() => Maybe.Some<double>(Value);

        /// <inheritdoc/>
        public Maybe<DateTime?> ToDateTime() => Maybe.None<DateTime?>();

        /// <inheritdoc/>
        public Maybe<TimeSpan> ToTimeSpan() => Maybe.None<TimeSpan>();

        /// <inheritdoc/>
        public Maybe<IReadOnlyList<ITpsObject>> ToArray()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

        /// <inheritdoc/>
        public string ToString(string format) => Value.ToString(format, CultureInfo.InvariantCulture);

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is TpsShort x && Equals(x);

        /// <inheritdoc/>
        public bool Equals(TpsShort other) =>
            Value == other.Value;

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return -1937169414 + Value.GetHashCode();
        }

        /// <inheritdoc/>
        public static bool operator ==(TpsShort left, TpsShort right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(TpsShort left, TpsShort right) => !(left == right);
    }
}
