using System;
using System.Collections.Generic;
using System.Globalization;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a signed 32-bit integer.
    /// </summary>
    public readonly struct TpsLong : ITpsObject, IEquatable<TpsLong>
    {
        /// <inheritdoc/>
        public TpsTypeCode TypeCode => TpsTypeCode.Long;

        private int Value { get; }

        /// <summary>
        /// Instantiates a new LONG value from the given value.
        /// </summary>
        /// <param name="value"></param>
        public TpsLong(int value)
        {
            Value = value;
        }

        /// <summary>
        /// Returns true if the value is not zero.
        /// </summary>
        public Maybe<bool> ToBoolean() => Maybe.Some(Value != 0);

        /// <summary>
        /// Gets a <see cref="DateTime"/> by treating the value as a Clarion Standard Date, where the value is the number of days since <see cref="TpsDate.ClarionEpoch"/>.
        /// For more information about the Clarion Standard Date, see the remarks section of <see cref="TpsDate"/>.
        /// </summary>
        public Maybe<DateTime?> ToDateTime() => Maybe.Some<DateTime?>(TpsDate.ClarionEpoch.AddDays(Value));

        /// <summary>
        /// Gets a <see cref="TimeSpan"/> by treating the value as a Clarion Standard Time, where the value is the number of centiseconds (1/100 seconds) since midnight.
        /// For more information about the Clarion Standard Time, see the remarks section of <see cref="TpsTime"/>.
        /// </summary>
        public Maybe<TimeSpan> ToTimeSpan() => Maybe.Some(new TimeSpan(0, 0, 0, 0, Value * 10));

        /// <inheritdoc/>
        public Maybe<uint> ToUInt32() =>
            Value < 0
            ? Maybe.None<uint>()
            : Maybe.Some((uint)Value);

        /// <inheritdoc/>
        public Maybe<int> ToInt32() => Maybe.Some(Value);

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
        public string ToString(string format) => Value.ToString(format, CultureInfo.InvariantCulture);

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
        public Maybe<short> ToInt16() =>
            short.MinValue > Value || short.MaxValue < Value
            ? Maybe.None<short>()
            : Maybe.Some((short)Value);

        /// <inheritdoc/>
        public Maybe<ushort> ToUInt16() =>
            ushort.MinValue > Value || ushort.MaxValue < Value
            ? Maybe.None<ushort>()
            : Maybe.Some((ushort)Value);

        /// <inheritdoc/>
        public Maybe<float> ToFloat() => Maybe.Some((float)Value);

        /// <inheritdoc/>
        public Maybe<double> ToDouble() => Maybe.Some((double)Value);

        /// <inheritdoc/>
        public Maybe<IReadOnlyList<ITpsObject>> ToArray() => Maybe.None<IReadOnlyList<ITpsObject>>();


        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is TpsLong x && Equals(x);

        /// <inheritdoc/>
        public bool Equals(TpsLong other) =>
            Value == other.Value;

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return -1937169414 + Value.GetHashCode();
        }

        /// <inheritdoc/>
        public static bool operator ==(TpsLong left, TpsLong right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(TpsLong left, TpsLong right) => !(left == right);
    }
}
