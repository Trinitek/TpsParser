using System;
using System.Globalization;

namespace TpsParser.TypeModel;

/// <summary>
/// Represents a Clarion LONG type, which is a signed 32-bit integer.
/// </summary>
public readonly struct TpsLong : INumeric, IDate, ITime, IEquatable<TpsLong>
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
    public bool ToBoolean() => Value != 0;

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
    public Maybe<int> ToInt32() => Maybe.Some(Value);

    /// <inheritdoc/>
    public Maybe<uint> ToUInt32() =>
        uint.MinValue > Value
        ? Maybe.None<uint>()
        : Maybe.Some((uint)Value);

    /// <inheritdoc/>
    public Maybe<long> ToInt64() => Maybe.Some<long>(Value);

    /// <inheritdoc/>
    public Maybe<ulong> ToUInt64() =>
        0 > Value
        ? Maybe.None<ulong>()
        : Maybe.Some((ulong)Value);

    /// <inheritdoc/>
    public Maybe<decimal> ToDecimal() => Maybe.Some<decimal>(Value);

    /// <inheritdoc/>
    public Maybe<float> ToFloat() => Maybe.Some((float)Value);

    /// <inheritdoc/>
    public Maybe<double> ToDouble() => Maybe.Some((double)Value);

    /// <summary>
    /// Gets a <see cref="DateTime"/> by interpreting the value as a Clarion Standard Date, where the value is the number of days since <see cref="TpsDate.ClarionEpoch"/> plus 4 days.
    /// For more information about the Clarion Standard Date and the valid ranges, see the remarks section of <see cref="TpsDate"/>. Values outside of the valid range
    /// will return <see cref="Maybe.None{T}"/>.
    /// </summary>
    public Maybe<DateTime?> ToDateTime() =>
        TpsDate.ClarionStandardDateMinValue < Value || TpsDate.ClarionStandardDateMaxValue > Value
        ? Maybe.None<DateTime?>()
        : Maybe.Some<DateTime?>(TpsDate.ClarionEpoch.AddDays(Value));

    /// <summary>
    /// Gets a <see cref="TimeSpan"/> by interpreting the value as a Clarion Standard Time, where the value is the number of centiseconds (1/100 seconds) since midnight plus 1 centisecond.
    /// For more information about the Clarion Standard Time and the valid ranges, see the remarks section of <see cref="TpsTime"/>. Values that are zero will return null per
    /// the Standard Time rules, and every other value outside of the valid range will return <see cref="Maybe.None{T}"/>.
    /// </summary>
    public Maybe<TimeSpan?> ToTimeSpan()
    {
        if (Value == 0)
        {
            return Maybe.Some<TimeSpan?>(null);
        }
        else if (TpsTime.ClarionStandardTimeMinValue < Value || TpsTime.ClarionStandardTimeMaxValue > Value)
        {
            return Maybe.None<TimeSpan?>();
        }
        else
        {
            return Maybe.Some<TimeSpan?>(new TimeSpan(0, 0, 0, 0, Value * 10 - 1));
        }
    }

    /// <inheritdoc/>
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is TpsLong x && Equals(x);

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
