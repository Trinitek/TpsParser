using System;

namespace TpsParser.TypeModel;

/// <summary>
/// Represents a Clarion <c>USHORT</c> type, which is an unsigned 16-bit integer.
/// </summary>
public readonly struct ClaUnsignedShort : IClaNumeric, IEquatable<ClaUnsignedShort>
{
    /// <inheritdoc/>
    public ClaTypeCode TypeCode => ClaTypeCode.UShort;

    /// <summary>
    /// Gets the .NET CLR value.
    /// </summary>
    public ushort Value { get; }

    /// <summary>
    /// Instantiates a new <c>USHORT</c>.
    /// </summary>
    /// <param name="value"></param>
    public ClaUnsignedShort(ushort value) => Value = value;

    /// <summary>
    /// Returns <see langword="true"/> if the value is not zero.
    /// </summary>
    /// <returns></returns>
    public bool ToBoolean() => Value != 0;

    /// <inheritdoc/>
    public Maybe<sbyte> ToSByte() =>
        sbyte.MaxValue < Value
        ? Maybe.None<sbyte>()
        : Maybe.Some((sbyte)Value);

    /// <inheritdoc/>
    public Maybe<byte> ToByte() =>
        byte.MaxValue < Value
        ? Maybe.None<byte>()
        : Maybe.Some((byte)Value);

    /// <inheritdoc/>
    public Maybe<ushort> ToUInt16() => Maybe.Some(Value);

    /// <inheritdoc/>
    public Maybe<short> ToInt16() =>
        short.MaxValue < Value
        ? Maybe.None<short>()
        : Maybe.Some((short)Value);

    /// <inheritdoc/>
    public Maybe<uint> ToUInt32() => Maybe.Some<uint>(Value);

    /// <inheritdoc/>
    public Maybe<int> ToInt32() => Maybe.Some<int>(Value);

    /// <inheritdoc/>
    public Maybe<ulong> ToUInt64() => Maybe.Some<ulong>(Value);

    /// <inheritdoc/>
    public Maybe<long> ToInt64() => Maybe.Some<long>(Value);

    /// <inheritdoc/>
    public Maybe<decimal> ToDecimal() => Maybe.Some<decimal>(Value);

    /// <inheritdoc/>
    public Maybe<float> ToFloat() => Maybe.Some<float>(Value);

    /// <inheritdoc/>
    public Maybe<double> ToDouble() => Maybe.Some<double>(Value);
    
    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ClaUnsignedShort x && Equals(x);

    /// <inheritdoc/>
    public bool Equals(ClaUnsignedShort other) =>
        Value == other.Value;

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return -1937169414 + Value.GetHashCode();
    }

    /// <inheritdoc/>
    public static bool operator ==(ClaUnsignedShort left, ClaUnsignedShort right) => Equals(left, right);

    /// <inheritdoc/>
    public static bool operator !=(ClaUnsignedShort left, ClaUnsignedShort right) => !(left == right);
}
