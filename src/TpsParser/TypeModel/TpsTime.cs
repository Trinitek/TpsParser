using System;

namespace TpsParser.TypeModel;

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
/// In the Clarion documentation, this type is often referred to as a Btrieve time, referring to the historical
/// <see href="https://en.wikipedia.org/wiki/Btrieve">Btrieve Record Manager</see> and is designed for interoperability with that and
/// other external systems.
/// </para>
/// <para>
/// The native time type used in the Clarion programming language when performing calculations is a LONG (<see cref = "TpsLong"/>).
/// This is called a Clarion Standard Time value and counts the number of centiseconds since midnight, plus 1 centisecond.
/// The valid Clarion Standard Time range is 00:00:00.00 through 23:59:59.99, that is, an inclusive numerical range from 1
/// to 8,640,000, with 0 used to represent a null value.
/// </para>
/// </remarks>
public readonly struct TpsTime : ITime, IEquatable<TpsTime>
{
    /// <summary>
    /// The maximum number of centiseconds this type can represent.
    /// For the maximum valid Clarion Standard Time value, see <see cref="ClarionStandardTimeMaxValue"/>.
    /// </summary>
    public static readonly int MaxTotalCentiseconds = 8640000 - 1;

    /// <summary>
    /// Gets the minimum valid value of a Clarion Standard Time, 00:00:00.00.
    /// </summary>
    public static readonly int ClarionStandardTimeMinValue = 1;

    /// <summary>
    /// Gets the maximum valid value of a Clarion Standard Time, 23:59:59.99.
    /// </summary>
    public static readonly int ClarionStandardTimeMaxValue = MaxTotalCentiseconds + 1;

    /// <summary>
    /// Gets the total number of centiseconds (hundredths of a second) since midnight.
    /// This is not a Clarion Standard Time value.
    /// </summary>
    public int TotalCentiseconds { get; }

    /// <summary>
    /// Gets the number of hours, between 0 and 23 inclusive, or null.
    /// </summary>
    public byte Hours => (byte)((TotalCentiseconds - 1) / (60 * 60 * 100));

    /// <summary>
    /// Gets the number of minutes, between 0 and 59 inclusive, or null.
    /// </summary>
    public byte Minutes => (byte)((TotalCentiseconds - 1) / (60 * 100) % 60);

    /// <summary>
    /// Gets the number of seconds, between 0 and 59 inclusive, or null.
    /// </summary>
    public byte Seconds => (byte)((TotalCentiseconds - 1) / 100 % 60);

    /// <summary>
    /// Gets the number of centiseconds (hundredths of a second), between 0 and 99 inclusive, or null.
    /// </summary>
    public byte Centiseconds => (byte)((TotalCentiseconds - 1) % 100);

    /// <inheritdoc/>
    public TpsTypeCode TypeCode => TpsTypeCode.Time;

    /// <summary>
    /// Instantiates a new TIME from the given total number of centiseconds (hundredths of a second) since midnight.
    /// </summary>
    /// <param name="totalCentiseconds">The number of centiseconds since midnight. Note that this is not a Clarion Standard Time value.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
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
            hours * 60 * 60 * 100
            + minutes * 60 * 100
            + seconds * 100
            + centiseconds;
    }

    /// <summary>
    /// Instantiates a new TIME from the given TimeSpan.
    /// </summary>
    /// <param name="timeSpan"></param>
    public TpsTime(TimeSpan timeSpan)
    {
        if (timeSpan < TimeSpan.Zero || timeSpan > new TimeSpan(0, 23, 59, 59, 990))
        {
            throw new ArgumentOutOfRangeException(nameof(timeSpan), "The TimeSpan must be between 00:00:00.00 and 23:59:59.99 inclusive.");
        }

        TotalCentiseconds =
            timeSpan.Hours * 60 * 60 * 100
            + timeSpan.Minutes * 60 * 100
            + timeSpan.Seconds * 100
            + timeSpan.Milliseconds / 10;
    }

    /// <inheritdoc/>
    public bool ToBoolean() => TotalCentiseconds != 0;

    /// <inheritdoc/>
    public Maybe<TimeSpan?> ToTimeSpan() => Maybe.Some<TimeSpan?>(new TimeSpan(0, Hours, Minutes, Seconds, Centiseconds * 10));

    /// <summary>
    /// Gets a <see cref="TpsLong"/> instance representing the Clarion Standard Time, or number of centiseconds since midnight plus 1.
    /// </summary>
    /// <returns></returns>
    public Maybe<TpsLong> AsClarionStandardTime() => Maybe.Some(new TpsLong(TotalCentiseconds + 1));

    /// <inheritdoc/>
    public override string ToString() => $"{Hours:00}:{Minutes:00}:{Seconds:00}.{Centiseconds:00}";

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
