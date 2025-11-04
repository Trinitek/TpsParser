using System;
using System.Globalization;

namespace TpsParser.TypeModel;

/// <summary>
/// Represents a Clarion <c>SHORT</c> type, which is a signed 16-bit integer.
/// </summary>
public readonly struct ClaShort : IClaNumeric, IEquatable<ClaShort>
{
    /// <inheritdoc/>
    public ClaTypeCode TypeCode => ClaTypeCode.Short;

    /// <summary>
    /// Gets the .NET CLR value.
    /// </summary>
    public short Value { get; }

    /// <summary>
    /// Instantiates a new <c>SHORT</c>.
    /// </summary>
    /// <param name="value"></param>
    public ClaShort(short value) => Value = value;

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
    public Maybe<ushort> ToUInt16() =>
        Value < 0
        ? Maybe.None<ushort>()
        : Maybe.Some((ushort)Value);

    /// <inheritdoc/>
    public Maybe<short> ToInt16() => Maybe.Some(Value);

    /// <inheritdoc/>
    public Maybe<uint> ToUInt32() =>
        Value < 0
        ? Maybe.None<uint>()
        : Maybe.Some((uint)Value);

    /// <inheritdoc/>
    public Maybe<int> ToInt32() => Maybe.Some<int>(Value);

    /// <inheritdoc/>
    public Maybe<ulong> ToUInt64() =>
        Value < 0
        ? Maybe.None<ulong>()
        : Maybe.Some((ulong)Value);

    /// <inheritdoc/>
    public Maybe<long> ToInt64() => Maybe.Some<long>(Value);

    /// <inheritdoc/>
    public Maybe<decimal> ToDecimal() => Maybe.Some<decimal>(Value);

    /// <inheritdoc/>
    public Maybe<float> ToFloat() => Maybe.Some<float>(Value);

    /// <inheritdoc/>
    public Maybe<double> ToDouble() => Maybe.Some<double>(Value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ClaShort x && Equals(x);

    /// <inheritdoc/>
    public bool Equals(ClaShort other) =>
        Value == other.Value;

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return -1937169414 + Value.GetHashCode();
    }

    /// <inheritdoc/>
    public static bool operator ==(ClaShort left, ClaShort right) => left.Equals(right);

    /// <inheritdoc/>
    public static bool operator !=(ClaShort left, ClaShort right) => !(left == right);
}
