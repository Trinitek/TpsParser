using System;
using System.Globalization;
using System.Text;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a 128-bit binary coded decimal that can hold up to 31 digits.
    /// </summary>
    public sealed class TpsDecimal : TpsObject<(ulong High, ulong Low, byte Places)>
    {
        /// <summary>
        /// The maximum allowable number of decimal digits.
        /// </summary>
        public const int MaxLength = 31;

        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.Decimal;

        /// <summary>
        /// Gets the upper 64 bits of the packed decimal, including the sign in the highest nibble.
        /// </summary>
        public ulong ValueHigh => Value.High;

        /// <summary>
        /// Gets the lower 64 bits of the packed decimal.
        /// </summary>
        public ulong ValueLow => Value.Low;

        /// <summary>
        /// Gets the number of decimal digits in the fractional portion.
        /// </summary>
        public byte Places => Value.Places;

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

        private decimal? _valueAsDecimal;

        /// <summary>
        /// Instantiates a new DECIMAL.
        /// </summary>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <param name="places"></param>
        public TpsDecimal(ulong high, ulong low, byte places)
        {
            if (places > MaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(places), $"Number of places must not exceed {MaxLength}.");
            }

            Value = (high, low, places);
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
            
            int leftPlaces = MaxLength - Places;

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
        public override Maybe<bool> ToBoolean() => new Maybe<bool>(!IsZero);

        /// <inheritdoc/>
        public override Maybe<sbyte> ToSByte() => new Maybe<sbyte>((sbyte)ToDecimal().Value);

        /// <inheritdoc/>
        public override Maybe<byte> ToByte() => new Maybe<byte>((byte)ToDecimal().Value);

        /// <inheritdoc/>
        public override Maybe<ushort> ToUInt16() => new Maybe<ushort>((ushort)ToDecimal().Value);

        /// <inheritdoc/>
        public override Maybe<short> ToInt16() => new Maybe<short>((short)ToDecimal().Value);

        /// <inheritdoc/>
        public override Maybe<uint> ToUInt32() => new Maybe<uint>((uint)ToDecimal().Value);

        /// <inheritdoc/>
        public override Maybe<int> ToInt32() => new Maybe<int>((int)ToDecimal().Value);

        /// <inheritdoc/>
        public override Maybe<ulong> ToUInt64() => new Maybe<ulong>((ulong)ToDecimal().Value);

        /// <inheritdoc/>
        public override Maybe<long> ToInt64() => new Maybe<long>((long)ToDecimal().Value);

        /// <summary>
        /// Gets the value as a <see cref="decimal"/>. This type allows values up to 31 figures which exceeds <see cref="decimal"/>'s 29, so precision loss is possible.
        /// </summary>
        public override Maybe<decimal> ToDecimal()
        {
            if (!_valueAsDecimal.HasValue)
            {
                _valueAsDecimal = decimal.Parse(ToString(), CultureInfo.InvariantCulture);
            }

            return new Maybe<decimal>(_valueAsDecimal.Value);
        }

        /// <summary>
        /// Gets a <see cref="DateTime"/> by treating the value as a Clarion Standard Date, where the value is the number of days since <see cref="TpsDate.ClarionEpoch"/>.
        /// For more information about the Clarion Standard Date, see the remarks section of <see cref="TpsDate"/>.
        /// </summary>
        public override Maybe<DateTime?> ToDateTime() => new Maybe<DateTime?>(TpsDate.ClarionEpoch.Add(new TimeSpan((int)ToDecimal().Value, 0, 0, 0)));

        /// <summary>
        /// Gets a string representation of the type returned by <see cref="ToDecimal"/>.
        /// </summary>
        public override string ToString(string format) => ToDecimal().Value.ToString(format, CultureInfo.InvariantCulture);
    }
}
