using System;
using System.Globalization;

namespace TpsParser.TypeModel;

/// <summary>
/// Represents a Clarion <c>BYTE</c> type.
/// </summary>
public readonly struct ClaByte : IClaNumeric, IEquatable<ClaByte>
{
    /// <inheritdoc/>
    public FieldTypeCode TypeCode => FieldTypeCode.Byte;

    /// <summary>
    /// Gets the .NET CLR value.
    /// </summary>
    public byte Value { get; }

    /// <summary>
    /// Instantiates a new <c>BYTE</c>.
    /// </summary>
    /// <param name="value"></param>
    public ClaByte(byte value) => Value = value;

    /// <summary>
    /// Returns <see langword="true"/> if the value is not zero.
    /// </summary>
    public bool ToBoolean() => Value != 0;

    /// <inheritdoc/>
    public Maybe<sbyte> ToSByte() =>
        sbyte.MaxValue < Value
        ? Maybe.None<sbyte>()
        : Maybe.Some((sbyte)Value);

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
    public Maybe<float> ToFloat() => Maybe.Some<float>(Value);

    /// <inheritdoc/>
    public Maybe<double> ToDouble() => Maybe.Some<double>(Value);

    /// <inheritdoc/>
    public Maybe<decimal> ToDecimal() => Maybe.Some<decimal>(Value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

    /// <inheritdoc/>
    public bool Equals(ClaByte other) =>
        Value == other.Value;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ClaByte x && Equals(x);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return -1937169414 + Value.GetHashCode();
    }

    /// <inheritdoc/>
    public static bool operator ==(ClaByte left, ClaByte right) => left.Equals(right);

    /// <inheritdoc/>
    public static bool operator !=(ClaByte left, ClaByte right) => !(left == right);
}
