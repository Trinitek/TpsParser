using System;

namespace TpsParser.TypeModel;

/// <summary>
/// Represents a Clarion <c>TIME</c> type, which represents a moment in time.
/// Some time keeping fields you expect to be of type <see cref="ClaTime"/> may actually be of type <see cref="ClaLong"/>.
/// See the remarks section for details.
/// </summary>
/// <remarks>
/// <para>
/// A <c>TIME</c> is composed of 4 bytes. Each field corresponds to a byte as follows:
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
/// The native time type used in the Clarion programming language when performing calculations is a <c>LONG</c> (<see cref="ClaLong"/>).
/// This is called a Clarion Standard Time value and counts the number of centiseconds since midnight, plus 1 centisecond.
/// The valid Clarion Standard Time range is 00:00:00.00 through 23:59:59.99, that is, an inclusive numerical range from 1
/// to 8,640,000, with 0 used to represent a null value.
/// </para>
/// </remarks>
public readonly struct ClaTime : IClaTime, IEquatable<ClaTime>
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
    /// This is <b>not</b> a Clarion Standard Time value.
    /// </summary>
    public int TotalCentiseconds { get; }

    /// <summary>
    /// Gets the number of hours between 0 and 23 inclusive.
    /// </summary>
    public byte Hours => (byte)(TotalCentiseconds / (60 * 60 * 100));

    /// <summary>
    /// Gets the number of minutes between 0 and 59 inclusive.
    /// </summary>
    public byte Minutes => (byte)(TotalCentiseconds / (60 * 100) % 60);

    /// <summary>
    /// Gets the number of seconds between 0 and 59 inclusive.
    /// </summary>
    public byte Seconds => (byte)(TotalCentiseconds / 100 % 60);

    /// <summary>
    /// Gets the number of centiseconds (hundredths of a second) between 0 and 99 inclusive.
    /// </summary>
    public byte Centiseconds => (byte)(TotalCentiseconds % 100);

    /// <summary>
    /// Gets the .NET CLR value.
    /// </summary>
    public TimeOnly Value => new(hour: Hours, minute: Minutes, second: Seconds, millisecond: Centiseconds * 10);

    /// <inheritdoc/>
    public FieldTypeCode TypeCode => FieldTypeCode.Time;

    /// <summary>
    /// Instantiates a new <c>TIME</c> from the given total number of centiseconds (hundredths of a second) since midnight.
    /// </summary>
    /// <param name="totalCentiseconds">The number of centiseconds since midnight. Note that this is <b>not</b> a Clarion Standard Time value.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public ClaTime(int totalCentiseconds)
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
    public ClaTime(byte hours, byte minutes, byte seconds, byte centiseconds)
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
    /// Instantiates a new <c>TIME</c> from the given <see cref="TimeOnly"/>.
    /// </summary>
    /// <param name="timeOnly"></param>
    public ClaTime(TimeOnly timeOnly)
    {
        if (timeOnly > new TimeOnly(hour: 23, minute: 59, second: 59, millisecond: 990))
        {
            throw new ArgumentOutOfRangeException(nameof(timeOnly), "The TimeSpan must be between 00:00:00.00 and 23:59:59.99 inclusive.");
        }

        TotalCentiseconds =
            timeOnly.Hour * 60 * 60 * 100
            + timeOnly.Minute * 60 * 100
            + timeOnly.Second * 100
            + timeOnly.Millisecond / 10;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the total number of centiseconds is not zero.
    /// </summary>
    /// <returns></returns>
    public bool ToBoolean() => TotalCentiseconds != 0;

    /// <inheritdoc/>
    public Maybe<TimeOnly?> ToTimeOnly() => Maybe.Some<TimeOnly?>(Value);

    /// <summary>
    /// Gets a <see cref="ClaLong"/> instance representing the Clarion Standard Time, or number of centiseconds since midnight plus 1.
    /// </summary>
    /// <returns></returns>
    public Maybe<ClaLong> AsClarionStandardTime() => Maybe.Some(new ClaLong(TotalCentiseconds + 1));

    /// <inheritdoc/>
    public override string ToString() => $"{Hours:00}:{Minutes:00}:{Seconds:00}.{Centiseconds:00}";

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ClaTime x && Equals(x);

    /// <inheritdoc/>
    public bool Equals(ClaTime other) =>
        TotalCentiseconds == other.TotalCentiseconds;

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return -571326905 + TotalCentiseconds.GetHashCode();
    }

    /// <inheritdoc/>
    public static bool operator ==(ClaTime left, ClaTime right) => left.Equals(right);

    /// <inheritdoc/>
    public static bool operator !=(ClaTime left, ClaTime right) => !(left == right);
}
