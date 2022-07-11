﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a 128-bit binary coded decimal that can hold up to 31 digits.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The native value of this type is a <see cref="ValueTuple"/> that contains two <see cref="ulong"/> values
    /// that represent the digits and a <see cref="byte"/> for the number of digits in the fractional portion.
    /// </para>
    /// <para>
    /// The 31 digits are contained in the lower 124 bits, where <see cref="ValueHigh"/> and <see cref="ValueLow"/> are treated
    /// as a contiguous 128-bit value, and the most significant 4 bits represent the sign: 0 is positive,
    /// and every other value is negative.
    /// </para>
    /// <para>
    /// This type can contain numbers that are too large to convert to a <see cref="decimal"/> using <see cref="ToDecimal"/>.
    /// If you need to handle values with more than 27 digits, consider using <see cref="ToString()"/> instead.
    /// </para>
    /// </remarks>
    public readonly struct TpsDecimal : ITpsObject, IEquatable<TpsDecimal>
    {
        /// <summary>
        /// The maximum allowable number of decimal digits.
        /// </summary>
        public const int MaxLength = 31;

        /// <inheritdoc/>
        public TpsTypeCode TypeCode => TpsTypeCode.Decimal;

        /// <summary>
        /// Gets the upper 64 bits of the packed decimal, including the sign in the highest nibble.
        /// </summary>
        public ulong ValueHigh { get; }

        /// <summary>
        /// Gets the lower 64 bits of the packed decimal.
        /// </summary>
        public ulong ValueLow { get; }

        /// <summary>
        /// Gets the number of decimal digits in the fractional portion.
        /// </summary>
        public byte Scale { get; }

        /// <summary>
        /// Returns true if positive.
        /// </summary>
        public bool IsPositive => (ValueHigh & 0xF000_0000_0000_0000) == 0;

        /// <summary>
        /// Returns true if negative.
        /// </summary>
        public bool IsNegative => !IsPositive;

        /// <summary>
        /// Returns true if zero.
        /// </summary>
        public bool IsZero => ValueLow == 0 && (ValueHigh & 0x0FFF_FFFF_FFFF_FFFF) == 0;

        /// <summary>
        /// Instantiates a new DECIMAL.
        /// </summary>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <param name="scale"></param>
        public TpsDecimal(ulong high, ulong low, byte scale)
        {
            if (scale > MaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(scale), $"Number of places must not exceed {MaxLength}.");
            }

            ValueHigh = high;
            ValueLow = low;
            Scale = scale;
        }

        /// <summary>
        /// Parses the given string using invariant culture rules.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TpsDecimal Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"'{nameof(value)}' cannot be null or whitespace.", nameof(value));
            }
            if (value.Length > MaxLength + 2)
            {
                throw new ArgumentException($"String must be {MaxLength + 2} characters or less.", nameof(value));
            }

            ulong high = default;
            ulong low = default;
            byte? places = default;

            ref ulong current = ref low;

            bool signSet = false;
            int digit = 0;
            int shift = 0;

            for (int i = value.Length - 1; i >= 0; i--)
            {
                char c = value[i];

                switch (c)
                {
                    case '.':
                        {
                            if (places.HasValue)
                            {
                                goto default;
                            }

                            places = (byte)(value.Length - 1 - i);
                            break;
                        }
                    case '-':
                        {
                            if (signSet)
                            {
                                goto default;
                            }

                            high |= 0xF000_0000_0000_0000;
                            signSet = true;
                            break;
                        }
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        {
                            current |= ((ulong)c - '0') << (4 * shift);

                            digit++;

                            if (digit == 16)
                            {
                                shift = 0;
                                current = ref high;
                            }
                            else
                            {
                                shift++;
                            }

                            break;
                        }
                    default:
                        throw new FormatException($"Unexpected character '{c}' at index {i} when parsing string as {nameof(TpsDecimal)}");
                }
            }

            return new TpsDecimal(high, low, places ?? 0);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var sb = new StringBuilder();

            if (IsNegative)
            {
                sb.Append('-');
            }

            bool leadingZeroHandled = false;
            
            int leftPlaces = MaxLength - Scale;

            if (leftPlaces == 0)
            {
                sb.Append('0');
                leadingZeroHandled = true;
            }

            ulong current = ValueHigh << 4;

            Iterate(15);

            current = ValueLow;

            Iterate(16);

            if (!leadingZeroHandled)
            {
                sb.Append('0');
            }

            return sb.ToString();

            void Iterate(int ix)
            {
                for (int i = 0; i < ix; i++)
                {
                    if (leftPlaces == 0)
                    {
                        if (!leadingZeroHandled)
                        {
                            sb.Append('0');
                        }

                        sb.Append('.');
                        leadingZeroHandled = true;
                    }

                    leftPlaces--;

                    ulong digit = (current >> 60) & 0x0F;

                    if (digit == 0)
                    {
                        if (leadingZeroHandled)
                        {
                            sb.Append('0');
                            leadingZeroHandled = true;
                        }
                    }
                    else
                    {
                        char c = (char)('0' + (char)(int)digit);
                        sb.Append(c);
                        leadingZeroHandled = true;
                    }

                    current <<= 4;
                }
            }
        }

        /// <summary>
        /// Returns true if the value is not zero.
        /// </summary>
        public Maybe<bool> ToBoolean() => Maybe.Some(!IsZero);

        /// <inheritdoc/>
        public Maybe<sbyte> ToSByte()
        {
            decimal d = ToDecimal().Value;
            return sbyte.MinValue > d || sbyte.MaxValue < d
                ? Maybe.None<sbyte>()
                : Maybe.Some((sbyte)d);
        }

        /// <inheritdoc/>
        public Maybe<byte> ToByte()
        {
            decimal d = ToDecimal().Value;
            return byte.MinValue > d || byte.MaxValue < d
                ? Maybe.None<byte>()
                : Maybe.Some((byte)d);
        }

        /// <inheritdoc/>
        public Maybe<ushort> ToUInt16()
        {
            decimal d = ToDecimal().Value;
            return ushort.MinValue > d || ushort.MaxValue < d
                ? Maybe.None<ushort>()
                : Maybe.Some((ushort)d);
        }

        /// <inheritdoc/>
        public Maybe<short> ToInt16()
        {
            decimal d = ToDecimal().Value;
            return short.MinValue > d || short.MaxValue < d
                ? Maybe.None<short>()
                : Maybe.Some((short)d);
        }

        /// <inheritdoc/>
        public Maybe<uint> ToUInt32()
        {
            decimal d = ToDecimal().Value;
            return uint.MinValue > d || uint.MaxValue < d
                ? Maybe.None<uint>()
                : Maybe.Some((uint)d);
        }

        /// <inheritdoc/>
        public Maybe<int> ToInt32()
        {
            decimal d = ToDecimal().Value;
            return int.MinValue > d || int.MaxValue < d
                ? Maybe.None<int>()
                : Maybe.Some((int)d);
        }

        /// <inheritdoc/>
        public Maybe<ulong> ToUInt64()
        {
            decimal d = ToDecimal().Value;
            return ulong.MinValue > d || ulong.MaxValue < d
                ? Maybe.None<ulong>()
                : Maybe.Some((ulong)d);
        }

        /// <inheritdoc/>
        public Maybe<long> ToInt64()
        {
            decimal d = ToDecimal().Value;
            return long.MinValue > d || long.MaxValue < d
                ? Maybe.None<long>()
                : Maybe.Some((long)d);
        }

        /// <summary>
        /// Gets the value as a <see cref="decimal"/>. This type allows values up to 31 figures which exceeds <see cref="decimal"/>'s 29, so precision loss is possible.
        /// </summary>
        public Maybe<decimal> ToDecimal() =>
            (Scale <= 28 && true)
                ? Maybe.Some(new decimal(
                    lo: 0,
                    mid: 0,
                    hi: 0,
                    isNegative: IsNegative,
                    scale: Scale))
                : Maybe.None<decimal>();

        /// <summary>
        /// Gets a <see cref="DateTime"/> by treating the value as a Clarion Standard Date, where the value is the number of days since <see cref="TpsDate.ClarionEpoch"/>.
        /// For more information about the Clarion Standard Date, see the remarks section of <see cref="TpsDate"/>.
        /// </summary>
        public Maybe<DateTime?> ToDateTime()
        {
            var i = ToInt32();
            return i.HasValue
                ? Maybe.Some<DateTime?>(TpsDate.ClarionEpoch.Add(new TimeSpan(i.Value, 0, 0, 0)))
                : Maybe.None<DateTime?>();
        }

        /// <summary>
        /// Gets a string representation of the type returned by <see cref="ToDecimal"/>.
        /// </summary>
        public string ToString(string format) => ToDecimal().Value.ToString(format, CultureInfo.InvariantCulture);

        /// <inheritdoc/>
        public Maybe<float> ToFloat() => ToDecimal().ConvertSome(d => (float)d);

        /// <inheritdoc/>
        public Maybe<double> ToDouble() => ToDecimal().ConvertSome(d => (double)d);

        /// <inheritdoc/>
        public Maybe<TimeSpan> ToTimeSpan() => Maybe.None<TimeSpan>();

        /// <inheritdoc/>
        public Maybe<IReadOnlyList<ITpsObject>> ToArray() => Maybe.None<IReadOnlyList<ITpsObject>>();

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is TpsDecimal x && Equals(x);

        /// <inheritdoc/>
        public bool Equals(TpsDecimal other) =>
            ValueHigh == other.ValueHigh
            && ValueLow == other.ValueLow
            && Scale == other.Scale;

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = -1054985165;
            hashCode = hashCode * -1521134295 + ValueHigh.GetHashCode();
            hashCode = hashCode * -1521134295 + ValueLow.GetHashCode();
            hashCode = hashCode * -1521134295 + Scale.GetHashCode();
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(TpsDecimal left, TpsDecimal right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(TpsDecimal left, TpsDecimal right) => !(left == right);
    }
}
