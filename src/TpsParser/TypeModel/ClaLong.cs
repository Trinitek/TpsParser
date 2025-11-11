using System;
using System.Globalization;

namespace TpsParser.TypeModel;

/// <summary>
/// Represents a Clarion <c>LONG</c> type, which is a signed 32-bit integer.
/// </summary>
/// <remarks>
/// Neither the TopSpeed file format nor the 32-bit Clarion runtime support 64-bit integer types.
/// The Clarion programming language originated on the 16-bit MS-DOS environment where the native integer size was 16-bits wide with types
/// <c>SHORT</c> (<see cref="ClaShort"/>) and <c>USHORT</c> (<see cref="ClaUnsignedShort"/>). The names of their wider 32-bit counterparts followed as
/// <c>LONG</c> (<see cref="ClaLong"/>) and <c>ULONG</c> (<see cref="ClaUnsignedLong"/>).
/// </remarks>
public readonly struct ClaLong : IClaNumeric, IClaDate, IClaTime, IEquatable<ClaLong>
{
    /// <inheritdoc/>
    public FieldTypeCode TypeCode => FieldTypeCode.Long;

    /// <summary>
    /// Gets the .NET CLR value.
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Instantiates a new <c>LONG</c> value from the given value.
    /// </summary>
    /// <param name="value"></param>
    public ClaLong(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the value is not zero.
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
    /// Gets a <see cref="DateOnly"/> by interpreting the value as a Clarion Standard Date, where the value is the number of days since <see cref="ClaDate.ClarionEpoch"/> plus 4 days.
    /// For more information about the Clarion Standard Date and the valid ranges, see the remarks section of <see cref="ClaDate"/>. Values outside of the valid range
    /// will return <see cref="Maybe.None{T}"/>.
    /// </summary>
    public Maybe<DateOnly?> ToDateOnly() =>
        ClaDate.ClarionStandardDateMinValue < Value || ClaDate.ClarionStandardDateMaxValue > Value
        ? Maybe.None<DateOnly?>()
        : Maybe.Some<DateOnly?>(ClaDate.ClarionEpoch.AddDays(Value));

    /// <summary>
    /// Gets a <see cref="TimeOnly"/> by interpreting the value as a Clarion Standard Time, where the value is the number of centiseconds (1/100 seconds) since midnight plus 1 centisecond.
    /// For more information about the Clarion Standard Time and the valid ranges, see the remarks section of <see cref="ClaTime"/>. Values that are zero will return <see langword="null"/> per
    /// the Standard Time rules, and every other value outside of the valid range will return <see cref="Maybe.None{T}"/>.
    /// </summary>
    public Maybe<TimeOnly?> ToTimeOnly()
    {
        if (Value == 0)
        {
            return Maybe.Some<TimeOnly?>(null);
        }
        else if (ClaTime.ClarionStandardTimeMinValue < Value || ClaTime.ClarionStandardTimeMaxValue > Value)
        {
            return Maybe.None<TimeOnly?>();
        }
        else
        {
            return Maybe.Some<TimeOnly?>(new TimeOnly(0, 0, 0, millisecond: Value * 10 - 1));
        }
    }

    /// <inheritdoc/>
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ClaLong x && Equals(x);

    /// <inheritdoc/>
    public bool Equals(ClaLong other) =>
        Value == other.Value;

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return -1937169414 + Value.GetHashCode();
    }

    /// <inheritdoc/>
    public static bool operator ==(ClaLong left, ClaLong right) => left.Equals(right);

    /// <inheritdoc/>
    public static bool operator !=(ClaLong left, ClaLong right) => !(left == right);
}
