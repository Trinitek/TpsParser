﻿using System;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents an unsigned short.
    /// </summary>
    public readonly struct TpsUnsignedShort : ISimple, IEquatable<TpsUnsignedShort>
    {
        /// <inheritdoc/>
        public TpsTypeCode TypeCode => TpsTypeCode.UShort;

        private ushort Value { get; }

        /// <summary>
        /// Instantiates a new USHORT.
        /// </summary>
        /// <param name="value"></param>
        public TpsUnsignedShort(ushort value) => Value = value;

        /// <inheritdoc/>
        public Maybe<bool> ToBoolean() => Maybe.Some(Value != 0);

        /// <inheritdoc/>
        public Maybe<ushort> ToUInt16() => Maybe.Some(Value);

        /// <inheritdoc/>
        public Maybe<short> ToInt16() => Maybe.Some((short)Value);

        /// <inheritdoc/>
        public Maybe<uint> ToUInt32() => Maybe.Some<uint>(Value);

        /// <inheritdoc/>
        public Maybe<int> ToInt32() => Maybe.Some<int>(Value);

        /// <inheritdoc/>
        public Maybe<ulong> ToUInt64() => Maybe.Some<ulong>(Value);

        /// <inheritdoc/>
        public Maybe<long> ToInt64() => Maybe.Some<long>(Value);

        /// <inheritdoc/>
        public Maybe<decimal> ToDecimal() => Maybe.Some<decimal>(Value);

        /// <inheritdoc/>
        public Maybe<sbyte> ToSByte() =>
            sbyte.MaxValue < Value
            ? Maybe.None<sbyte>()
            : Maybe.Some((sbyte)Value);

        /// <inheritdoc/>
        public Maybe<byte> ToByte() =>
            byte.MaxValue < Value
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
        public override bool Equals(object obj) => obj is TpsUnsignedShort x && Equals(x);

        /// <inheritdoc/>
        public bool Equals(TpsUnsignedShort other) =>
            Value == other.Value;

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return -1937169414 + Value.GetHashCode();
        }

        /// <inheritdoc/>
        public static bool operator ==(TpsUnsignedShort left, TpsUnsignedShort right) => Equals(left, right);

        /// <inheritdoc/>
        public static bool operator !=(TpsUnsignedShort left, TpsUnsignedShort right) => !(left == right);
    }
}
