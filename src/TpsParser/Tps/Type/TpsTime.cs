using System;
using System.Collections.Generic;
using System.Globalization;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a moment in time. Some time keeping fields you expect to be of type <see cref="TpsTime"/> may actually be of type <see cref="TpsLong"/>.
    /// See the remarks section for details.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A TIME is composed of 4 bytes. Each field corresponds to a byte as follows:
    /// <list type="table">
    /// <item>
    /// <term>Centiseconds</term>
    /// <description>0 to 99</description>
    /// </item>
    /// <item>
    /// <term>Seconds</term>
    /// <description>0 to 59</description>
    /// </item>
    /// <item>
    /// <term>Minutes</term>
    /// <description>0 to 59</description>
    /// </item>
    /// <item>
    /// <term>Hours</term>
    /// <description>0 to 23</description>
    /// </item>
    /// </list>
    /// A centisecond is 1/100th of a second.
    /// </para>
    /// <para>
    /// In the Clarion programming language, when a TIME value is used in an expression, it is implicitly converted to a
    /// LONG (see <see cref="TpsLong"/>). This is called a Clarion Standard Time value. This value is the number of centiseconds since midnight.
    /// </para>
    /// <para>
    /// Clarion documentation recommends that the 4-byte TIME type be used when communicating with external applications. However, because
    /// TopSpeed files are typically only used by Clarion applications exclusively, the field may sometimes be defined as a LONG instead.
    /// </para>
    /// </remarks>
    public readonly struct TpsTime : ITpsObject, IEquatable<TpsTime>
    {
        /// <summary>
        /// The maximum number of centiseconds this type can represent.
        /// </summary>
        public const int MaxTotalCentiseconds = 8639999;

        /// <summary>
        /// Gets the total number of centiseconds (hundredths of a second) since midnight.
        /// </summary>
        public int TotalCentiseconds { get; }

        /// <summary>
        /// Gets the number of hours, between 0 and 23 inclusive.
        /// </summary>
        public byte Hours => (byte)(TotalCentiseconds / (60 * 60 * 100));

        /// <summary>
        /// Gets the number of minutes, between 0 and 59 inclusive.
        /// </summary>
        public byte Minutes => (byte)(TotalCentiseconds / (60 * 100) % 60);

        /// <summary>
        /// Gets the number of seconds, between 0 and 59 inclusive.
        /// </summary>
        public byte Seconds => (byte)((TotalCentiseconds / 100) % 60);

        /// <summary>
        /// Gets the number of centiseconds (hundredths of a second), between 0 and 99 inclusive.
        /// </summary>
        public byte Centiseconds => (byte)(TotalCentiseconds % 100);

        /// <inheritdoc/>
        public TpsTypeCode TypeCode => TpsTypeCode.Time;

        /// <summary>
        /// Instantiates a new TIME from the given total number of centiseconds (hundredths of a second) since midnight.
        /// </summary>
        /// <param name="totalCentiseconds"></param>
        public TpsTime(int totalCentiseconds)
        {
            if (totalCentiseconds < 0 || totalCentiseconds > MaxTotalCentiseconds)
            {
                throw new ArgumentOutOfRangeException(nameof(totalCentiseconds), $"Total centiseconds must be between 0 and {MaxTotalCentiseconds} inclusive, but was {totalCentiseconds}.");
            }

            TotalCentiseconds = totalCentiseconds;
        }

        /// <summary>
        /// Instantiates a new TIME from the given hour, minute, second, and centisecond components.
        /// </summary>
        /// <param name="hours">Hours since midnight. Must be between 0 and 23 inclusive.</param>
        /// <param name="minutes">Minutes since midnight. Must be between 0 and 59 inclusive.</param>
        /// <param name="seconds">Seconds since midnight. Must be between 0 and 59 inclusive.</param>
        /// <param name="centiseconds">Centiseconds (hundredths of a second) since midnight. Must be between 0 and 99 inclusive.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public TpsTime(byte hours, byte minutes, byte seconds, byte centiseconds)
        {
            if (hours > 23)
            {
                throw new ArgumentOutOfRangeException(nameof(hours), $"Hours must be between 0 and 23 inclusive, but was {hours}.");
            }
            if (minutes > 59)
            {
                throw new ArgumentOutOfRangeException(nameof(minutes), $"Minutes must be between 0 and 59 inclusive, but was {minutes}.");
            }
            if (seconds > 59)
            {
                throw new ArgumentOutOfRangeException(nameof(seconds), $"Seconds must be between 0 and 59 inclusive, but was {seconds}.");
            }
            if (centiseconds > 99)
            {
                throw new ArgumentOutOfRangeException(nameof(centiseconds), $"Centiseconds must be between 0 and 99 inclusive, but was {centiseconds}.");
            }

            TotalCentiseconds =
                (hours * 60 * 60 * 100)
                + (minutes * 60 * 100)
                + (seconds * 100)
                + centiseconds;
        }

        /// <summary>
        /// Returns true if the value is not equal to <see cref="TimeSpan.Zero"/>.
        /// </summary>
        public Maybe<bool> ToBoolean() => Maybe.Some(TotalCentiseconds != 0);

        /// <inheritdoc/>
        public Maybe<TimeSpan> ToTimeSpan() => Maybe.Some<TimeSpan>(new TimeSpan(0, Hours, Minutes, Seconds, Centiseconds * 10));

        /// <inheritdoc/>
        public string ToString(string format) => ToTimeSpan().Value.ToString(format, CultureInfo.InvariantCulture);

        /// Gets the number of centiseconds since midnight, or an empty <see cref="Maybe{T}"/> if the number is too large to fit the target type.
        public Maybe<sbyte> ToSByte() =>
            sbyte.MinValue > TotalCentiseconds || sbyte.MaxValue < TotalCentiseconds
            ? Maybe.None<sbyte>()
            : Maybe.Some((sbyte)TotalCentiseconds);

        /// Gets the number of centiseconds since midnight, or an empty <see cref="Maybe{T}"/> if the number is too large to fit the target type.
        public Maybe<byte> ToByte() =>
            byte.MinValue > TotalCentiseconds || byte.MaxValue < TotalCentiseconds
            ? Maybe.None<byte>()
            : Maybe.Some((byte)TotalCentiseconds);

        /// Gets the number of centiseconds since midnight, or an empty <see cref="Maybe{T}"/> if the number is too large to fit the target type.
        public Maybe<short> ToInt16() =>
            short.MinValue > TotalCentiseconds || short.MaxValue < TotalCentiseconds
            ? Maybe.None<short>()
            : Maybe.Some((short)TotalCentiseconds);

        /// Gets the number of centiseconds since midnight, or an empty <see cref="Maybe{T}"/> if the number is too large to fit the target type.
        public Maybe<ushort> ToUInt16() =>
            ushort.MinValue > TotalCentiseconds || ushort.MaxValue < TotalCentiseconds
            ? Maybe.None<ushort>()
            : Maybe.Some((ushort)TotalCentiseconds);

        /// Gets the number of centiseconds since midnight.
        public Maybe<int> ToInt32() => Maybe.Some(TotalCentiseconds);

        /// Gets the number of centiseconds since midnight.
        public Maybe<uint> ToUInt32() => Maybe.Some((uint)TotalCentiseconds);

        /// Gets the number of centiseconds since midnight.
        public Maybe<long> ToInt64() => Maybe.Some((long)TotalCentiseconds);

        /// Gets the number of centiseconds since midnight.
        public Maybe<ulong> ToUInt64() => Maybe.Some((ulong)TotalCentiseconds);

        /// Gets the number of centiseconds since midnight.
        public Maybe<float> ToFloat() => Maybe.Some((float)TotalCentiseconds);

        /// Gets the number of centiseconds since midnight.
        public Maybe<double> ToDouble() => Maybe.Some((double)TotalCentiseconds);

        /// Gets the number of centiseconds since midnight.
        public Maybe<decimal> ToDecimal() => Maybe.Some((decimal)TotalCentiseconds);

        /// <inheritdoc/>
        public Maybe<DateTime?> ToDateTime() => Maybe.None<DateTime?>();

        /// <inheritdoc/>
        public Maybe<IReadOnlyList<ITpsObject>> ToArray() => Maybe.None<IReadOnlyList<ITpsObject>>();

        /// <summary>
        /// Gets a <see cref="TpsLong"/> instance representing the Clarion Standard Time, or number of centiseconds since midnight.
        /// </summary>
        /// <returns></returns>
        public Maybe<TpsLong> AsClarionStandardTime() => Maybe.Some(new TpsLong(TotalCentiseconds));

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is TpsTime x && Equals(x);

        /// <inheritdoc/>
        public bool Equals(TpsTime other) =>
            TotalCentiseconds == other.TotalCentiseconds;

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return -571326905 + TotalCentiseconds.GetHashCode();
        }

        /// <inheritdoc/>
        public static bool operator ==(TpsTime left, TpsTime right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(TpsTime left, TpsTime right) => !(left == right);
    }
}
