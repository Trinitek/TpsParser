using System;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents an unsigned 32-bit integer.
    /// </summary>
    public readonly struct TpsUnsignedLong : INumeric, IEquatable<TpsUnsignedLong>
    {
        /// <inheritdoc/>
        public TpsTypeCode TypeCode => TpsTypeCode.ULong;

        private uint Value { get; }

        /// <summary>
        /// Instantiates a new ULONG.
        /// </summary>
        /// <param name="value"></param>
        public TpsUnsignedLong(uint value)
        {
            Value = value;
        }

        /// <summary>
        /// Returns true if the value is not zero.
        /// </summary>
        public bool ToBoolean() => Value != 0;

        /// <inheritdoc/>
        public Maybe<sbyte> ToSByte() =>
            sbyte.MaxValue < Value
            ? Maybe.None<sbyte>()
            : Maybe.Some((sbyte)Value);

        /// <inheritdoc/>
        public Maybe<byte> ToByte() =>
            byte.MinValue > Value || byte.MaxValue < Value
            ? Maybe.None<byte>()
            : Maybe.Some((byte)Value);

        /// <inheritdoc/>
        public Maybe<short> ToInt16() =>
            short.MaxValue < Value
            ? Maybe.None<short>()
            : Maybe.Some((short)Value);

        /// <inheritdoc/>
        public Maybe<ushort> ToUInt16() =>
            ushort.MaxValue < Value
            ? Maybe.None<ushort>()
            : Maybe.Some((ushort)Value);

        /// <inheritdoc/>
        public Maybe<int> ToInt32() =>
            int.MaxValue < Value 
            ? Maybe.None<int>()
            : new Maybe<int>((int)Value);

        /// <inheritdoc/>
        public Maybe<uint> ToUInt32() => new Maybe<uint>(Value);

        /// <inheritdoc/>
        public Maybe<long> ToInt64() => new Maybe<long>(Value);

        /// <inheritdoc/>
        public Maybe<ulong> ToUInt64() => new Maybe<ulong>(Value);

        /// <inheritdoc/>
        public Maybe<decimal> ToDecimal() => new Maybe<decimal>(Value);

        /// <inheritdoc/>
        public Maybe<float> ToFloat()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Maybe<double> ToDouble()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool Equals(TpsUnsignedLong other) =>
            Value == other.Value;

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is TpsUnsignedLong x && Equals(x);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return -1937169414 + Value.GetHashCode();
        }

        /// <inheritdoc/>
        public static bool operator ==(TpsUnsignedLong left, TpsUnsignedLong right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(TpsUnsignedLong left, TpsUnsignedLong right) => !(left == right);
    }
}
