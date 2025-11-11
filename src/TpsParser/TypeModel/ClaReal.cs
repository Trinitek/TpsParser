using System;
using System.Globalization;

namespace TpsParser.TypeModel;

/// <summary>
/// Represents a Clarion <c>REAL</c> type, which is a double-precision floating point number.
/// </summary>
public readonly struct ClaReal : IClaNumeric, IEquatable<ClaReal>
{
    /// <inheritdoc/>
    public FieldTypeCode TypeCode => FieldTypeCode.Real;

    /// <summary>
    /// Gets the .NET CLR value.
    /// </summary>
    public double Value { get; }

    private bool IsNotNumeric => double.IsNaN(Value) || double.IsInfinity(Value);

    /// <summary>
    /// Instantiates a new <c>REAL</c>.
    /// </summary>
    /// <param name="value"></param>
    public ClaReal(double value)
    {
        Value = value;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the value is not zero.
    /// </summary>
    public bool ToBoolean() => Value != 0.0;

    /// <inheritdoc/>
    public Maybe<sbyte> ToSByte() =>
        IsNotNumeric || sbyte.MinValue > Value || sbyte.MaxValue < Value
        ? Maybe.None<sbyte>()
        : Maybe.Some((sbyte)Value);

    /// <inheritdoc/>
    public Maybe<byte> ToByte() =>
        IsNotNumeric || byte.MinValue > Value || byte.MaxValue < Value
        ? Maybe.None<byte>()
        : Maybe.Some((byte)Value);

    /// <inheritdoc/>
    public Maybe<short> ToInt16() =>
        IsNotNumeric || short.MinValue > Value || short.MaxValue < Value
        ? Maybe.None<short>()
        : Maybe.Some((short)Value);

    /// <inheritdoc/>
    public Maybe<ushort> ToUInt16() =>
        IsNotNumeric || ushort.MinValue > Value || ushort.MaxValue < Value
        ? Maybe.None<ushort>()
        : Maybe.Some((ushort)Value);

    /// <inheritdoc/>
    public Maybe<int> ToInt32() =>
        IsNotNumeric || int.MinValue > Value || int.MaxValue < Value
        ? Maybe.None<int>()
        : Maybe.Some((int)Value);

    /// <inheritdoc/>
    public Maybe<uint> ToUInt32() =>
        IsNotNumeric || uint.MinValue > Value || uint.MaxValue < Value
        ? Maybe.None<uint>()
        : Maybe.Some((uint)Value);

    /// <inheritdoc/>
    public Maybe<long> ToInt64() =>
        IsNotNumeric || long.MinValue > Value || long.MaxValue < Value
        ? Maybe.None<long>()
        : Maybe.Some((long)Value);

    /// <inheritdoc/>
    public Maybe<ulong> ToUInt64() =>
        IsNotNumeric || ulong.MinValue > Value || ulong.MaxValue < Value
        ? Maybe.None<ulong>()
        : Maybe.Some((ulong)Value);

    /// <inheritdoc/>
    public Maybe<float> ToFloat() => Maybe.Some((float)Value);

    /// <inheritdoc/>
    public Maybe<double> ToDouble() => new Maybe<double>(Value);

    /// <inheritdoc/>
    public Maybe<decimal> ToDecimal() =>
        IsNotNumeric || (double)decimal.MinValue > Value || (double)decimal.MaxValue < Value
        ? Maybe.None<decimal>()
        : Maybe.Some((decimal)Value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

    /// <inheritdoc/>
    public bool Equals(ClaReal other) =>
        Value == other.Value;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ClaReal x && Equals(x);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return -1937169414 + Value.GetHashCode();
    }

    /// <inheritdoc/>
    public static bool operator ==(ClaReal left, ClaReal right) => left.Equals(right);

    /// <inheritdoc/>
    public static bool operator !=(ClaReal left, ClaReal right) => !(left == right);
}
