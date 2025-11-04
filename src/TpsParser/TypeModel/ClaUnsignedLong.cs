using System;

namespace TpsParser.TypeModel;

/// <summary>
/// Represents a Clarion <c>ULONG</c> type, which is an unsigned 32-bit integer.
/// </summary>
/// <remarks>
/// The <c>LONG</c> name would appear to suggest that this type is a 64-bit integer; however, the 32-bit Clarion runtime does not support 64-bit integer types.
/// The Clarion programming language originated on the 16-bit MS-DOS environment where the native integer size was 16-bits wide with types
/// <c>SHORT</c> (<see cref="ClaShort"/>) and <c>USHORT</c> (<see cref="ClaUnsignedShort"/>). The names of their wider 32-bit counterparts followed as
/// <c>LONG</c> (<see cref="ClaLong"/>) and <c>ULONG</c> (<see cref="ClaUnsignedLong"/>).
/// </remarks>
public readonly struct ClaUnsignedLong : IClaNumeric, IEquatable<ClaUnsignedLong>
{
    /// <inheritdoc/>
    public ClaTypeCode TypeCode => ClaTypeCode.ULong;

    /// <summary>
    /// Gets the .NET CLR value.
    /// </summary>
    public uint Value { get; }

    /// <summary>
    /// Instantiates a new <c>ULONG</c>.
    /// </summary>
    /// <param name="value"></param>
    public ClaUnsignedLong(uint value)
    {
        Value = value;
    }

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
    public Maybe<byte> ToByte() =>
        byte.MinValue > Value || byte.MaxValue < Value
        ? Maybe.None<byte>()
        : Maybe.Some((byte)Value);

    /// <inheritdoc/>
    public Maybe<short> ToInt16() =>
        short.MaxValue < Value
        ? Maybe.None<short>()
        : Maybe.Some((short)Value);

    /// <inheritdoc/>
    public Maybe<ushort> ToUInt16() =>
        ushort.MaxValue < Value
        ? Maybe.None<ushort>()
        : Maybe.Some((ushort)Value);

    /// <inheritdoc/>
    public Maybe<int> ToInt32() =>
        int.MaxValue < Value 
        ? Maybe.None<int>()
        : new Maybe<int>((int)Value);

    /// <inheritdoc/>
    public Maybe<uint> ToUInt32() => Maybe.Some(Value);

    /// <inheritdoc/>
    public Maybe<long> ToInt64() => Maybe.Some<long>(Value);

    /// <inheritdoc/>
    public Maybe<ulong> ToUInt64() => Maybe.Some<ulong>(Value);

    /// <inheritdoc/>
    public Maybe<decimal> ToDecimal() => Maybe.Some<decimal>(Value);

    /// <inheritdoc/>
    public Maybe<float> ToFloat()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Maybe<double> ToDouble()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public bool Equals(ClaUnsignedLong other) =>
        Value == other.Value;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ClaUnsignedLong x && Equals(x);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return -1937169414 + Value.GetHashCode();
    }

    /// <inheritdoc/>
    public static bool operator ==(ClaUnsignedLong left, ClaUnsignedLong right) => left.Equals(right);

    /// <inheritdoc/>
    public static bool operator !=(ClaUnsignedLong left, ClaUnsignedLong right) => !(left == right);
}
