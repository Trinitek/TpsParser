﻿using System;
using System.Collections.Generic;
using System.Globalization;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a byte.
    /// </summary>
    public readonly struct TpsByte : ITpsObject, IEquatable<TpsByte>
    {
        /// <inheritdoc/>
        public TpsTypeCode TypeCode => TpsTypeCode.Byte;

        private byte Value { get; }

        /// <summary>
        /// Instantiates a new BYTE.
        /// </summary>
        /// <param name="value"></param>
        public TpsByte(byte value) => Value = value;

        /// <summary>
        /// Returns true if the value is not zero.
        /// </summary>
        public Maybe<bool> ToBoolean() => Maybe.Some(Value != 0);

        /// <inheritdoc/>
        public Maybe<sbyte> ToSByte() => Maybe.Some((sbyte)Value);

        /// <inheritdoc/>
        public Maybe<byte> ToByte() => Maybe.Some(Value);

        /// <inheritdoc/>
        public Maybe<ushort> ToUInt16() => Maybe.Some<ushort>(Value);

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
        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

        /// <inheritdoc/>
        public string ToString(string format) => Value.ToString(format, CultureInfo.InvariantCulture);

        /// <inheritdoc/>
        public Maybe<float> ToFloat() => Maybe.Some<float>(Value);

        /// <inheritdoc/>
        public Maybe<double> ToDouble() => Maybe.Some<double>(Value);

        /// <inheritdoc/>
        public Maybe<DateTime?> ToDateTime() => Maybe.None<DateTime?>();

        /// <inheritdoc/>
        public Maybe<TimeSpan> ToTimeSpan() => Maybe.None<TimeSpan>();

        /// <inheritdoc/>
        public Maybe<IReadOnlyList<ITpsObject>> ToArray() => Maybe.None<IReadOnlyList<ITpsObject>>();

        /// <inheritdoc/>
        public bool Equals(TpsByte other) =>
            Value == other.Value;

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is TpsByte x && Equals(x);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return -1937169414 + Value.GetHashCode();
        }

        /// <inheritdoc/>
        public static bool operator ==(TpsByte left, TpsByte right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(TpsByte left, TpsByte right) => !(left == right);
    }
}
