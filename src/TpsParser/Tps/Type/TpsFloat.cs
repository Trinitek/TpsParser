﻿using System;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a single-precision floating point number.
    /// </summary>
    public readonly struct TpsFloat : ISimple, IEquatable<TpsFloat>
    {
        /// <inheritdoc/>
        public TpsTypeCode TypeCode => TpsTypeCode.SReal;

        private float Value { get; }

        private bool IsNotNumeric => float.IsNaN(Value) || float.IsInfinity(Value);

        /// <summary>
        /// Instantiates a new SREAL.
        /// </summary>
        /// <param name="value"></param>
        public TpsFloat(float value)
        {
            Value = value;
        }

        /// <summary>
        /// Returns true if the value is not zero.
        /// </summary>
        public Maybe<bool> ToBoolean() => Maybe.Some(Value != 0.0f);

        /// <inheritdoc/>
        public Maybe<float> ToFloat() => Maybe.Some(Value);

        /// <inheritdoc/>
        public Maybe<double> ToDouble() => Maybe.Some<double>(Value);

        /// <inheritdoc/>
        public Maybe<decimal> ToDecimal() =>
            IsNotNumeric || (float)decimal.MinValue > Value || (float)decimal.MaxValue < Value
            ? Maybe.None<decimal>()
            : Maybe.Some((decimal)Value);

        /// <inheritdoc/>
        public Maybe<sbyte> ToSByte() =>
            IsNotNumeric || sbyte.MinValue > Value || sbyte.MaxValue < Value
            ? Maybe.None<sbyte>()
            : Maybe.Some((sbyte)Value);

        /// <inheritdoc/>
        public Maybe<byte> ToByte() =>
            IsNotNumeric || byte.MinValue > Value || byte.MaxValue < Value
            ? Maybe.None<byte>()
            : Maybe.Some((byte)Value);

        /// <inheritdoc/>
        public Maybe<short> ToInt16() =>
            IsNotNumeric || short.MinValue > Value || short.MaxValue < Value
            ? Maybe.None<short>()
            : Maybe.Some((short)Value);

        /// <inheritdoc/>
        public Maybe<ushort> ToUInt16() =>
            IsNotNumeric || ushort.MinValue > Value || ushort.MaxValue < Value
            ? Maybe.None<ushort>()
            : Maybe.Some((ushort)Value);

        /// <inheritdoc/>
        public Maybe<int> ToInt32() =>
            IsNotNumeric || int.MinValue > Value || int.MaxValue < Value
            ? Maybe.None<int>()
            : Maybe.Some((int)Value);

        /// <inheritdoc/>
        public Maybe<uint> ToUInt32() =>
            IsNotNumeric || uint.MinValue > Value || uint.MaxValue < Value
            ? Maybe.None<uint>()
            : Maybe.Some((uint)Value);

        /// <inheritdoc/>
        public Maybe<long> ToInt64() =>
            IsNotNumeric || long.MinValue > Value || long.MaxValue < Value
            ? Maybe.None<long>()
            : Maybe.Some((long)Value);

        /// <inheritdoc/>
        public Maybe<ulong> ToUInt64() =>
            IsNotNumeric || ulong.MinValue > Value || ulong.MaxValue < Value
            ? Maybe.None<ulong>()
            : Maybe.Some((ulong)Value);

        /// <inheritdoc/>
        public Maybe<DateTime?> ToDateTime() => Maybe.None<DateTime?>();

        /// <inheritdoc/>
        public Maybe<TimeSpan> ToTimeSpan() => Maybe.None<TimeSpan>();

        /// <inheritdoc/>
        public bool Equals(TpsFloat other) =>
            Value == other.Value;

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is TpsFloat x && Equals(x);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return -1937169414 + Value.GetHashCode();
        }

        /// <inheritdoc/>
        public static bool operator ==(TpsFloat left, TpsFloat right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(TpsFloat left, TpsFloat right) => !(left == right);
    }
}
