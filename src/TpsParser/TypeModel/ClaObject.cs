using System;
using System.Text;

namespace TpsParser.TypeModel;

/// <summary>
/// Represents a typed field value within the TopSpeed file.
/// <see cref="IClaObject"/> types are modeled to correspond directly to types found in the Clarion runtime.
/// </summary>
public interface IClaObject
{
    /// <summary>
    /// Gets the type code of the object.
    /// </summary>
    FieldTypeCode TypeCode { get; }
}

/// <summary>
/// Represents a Clarion type with boolean semantics.
/// </summary>
public interface IClaBoolean : IClaObject
{
    /// <summary>
    /// Gets a <see cref="bool"/> representation of the value as governed by Clarion logic evaluation rules for the type.
    /// For all numeric types, this will be <see langword="true"/> for all non-zero values.
    /// For all string types, this will be <see langword="true"/> for strings that have a length greater than zero and do not consist entirely of "blank" padding characters (ASCII 0x20, i.e. space).
    /// </summary>
    /// <returns></returns>
    bool ToBoolean();
}

/// <summary>
/// Represents a numeric Clarion type.
/// </summary>
public interface IClaNumeric : IClaBoolean
{
    /// <summary>
    /// Gets an <see cref="sbyte"/> representation of the value, if it can be converted.
    /// </summary>
    /// <returns></returns>
    Maybe<sbyte> ToSByte();

    /// <summary>
    /// Gets a <see cref="byte"/> representation of the value, if it can be converted.
    /// </summary>
    /// <returns></returns>
    Maybe<byte> ToByte();

    /// <summary>
    /// Gets a <see cref="short"/> representation of the value, if it can be converted.
    /// </summary>
    /// <returns></returns>
    Maybe<short> ToInt16();

    /// <summary>
    /// Gets a <see cref="uint"/> representation of the value, if it can be converted.
    /// </summary>
    /// <returns></returns>
    Maybe<ushort> ToUInt16();

    /// <summary>
    /// Gets an <see cref="int"/> representation of the value, if it can be converted.
    /// </summary>
    /// <returns></returns>
    Maybe<int> ToInt32();

    /// <summary>
    /// Gets a <see cref="uint"/> representation of the value, if it can be converted.
    /// </summary>
    /// <returns></returns>
    Maybe<uint> ToUInt32();

    /// <summary>
    /// Gets a <see cref="long"/> representation of the value, if it can be converted.
    /// </summary>
    /// <returns></returns>
    Maybe<long> ToInt64();

    /// <summary>
    /// Gets a <see cref="ulong"/> representation of the value, if it can be converted.
    /// </summary>
    /// <returns></returns>
    Maybe<ulong> ToUInt64();

    /// <summary>
    /// Gets a <see cref="float"/> representation of the value, if it can be converted.
    /// </summary>
    /// <returns></returns>
    Maybe<float> ToFloat();

    /// <summary>
    /// Gets a <see cref="double"/> representation of the value, if it can be converted.
    /// </summary>
    /// <returns></returns>
    Maybe<double> ToDouble();

    /// <summary>
    /// Gets a <see cref="decimal"/> representation of the value, if it can be converted.
    /// </summary>
    /// <returns></returns>
    Maybe<decimal> ToDecimal();
}

/// <summary>
/// Represents a Clarion string type.
/// </summary>
public interface IClaString : IClaBoolean
{
    /// <summary>
    /// Gets the <see cref="string"/> backing this type, if available.
    /// If <see langword="null"/>, the string content will be available in <see cref="ContentValue"/>.
    /// </summary>
    string? StringValue { get; }

    /// <summary>
    /// Gets the memory content backing this type, if available.
    /// If <see langword="null"/>, the string content will be available in <see cref="StringValue"/>.
    /// </summary>
    ReadOnlyMemory<byte>? ContentValue { get; }

    /// <summary>
    /// Returns <see langword="true"/> if the string's length is greater than zero and is not entirely filled with SPACE as padding.
    /// If <see cref="StringValue"/> is available, <paramref name="encoding"/> is ignored.
    /// Otherwise, the SPACE character is determined using the specified encoding against <see cref="ContentValue"/>.
    /// </summary>
    /// <param name="encoding"></param>
    /// <returns></returns>
    bool ToBoolean(Encoding encoding);

    /// <summary>
    /// Gets the string value stored in <see cref="StringValue"/> if available,
    /// or returns a new string from <see cref="ContentValue"/> using the given encoding.
    /// </summary>
    /// <param name="encoding"></param>
    /// <returns></returns>
    string ToString(Encoding encoding);
}

/// <summary>
/// Represents a Clarion type that can be interpreted as a point in time.
/// </summary>
public interface IClaTime : IClaObject
{
    /// <summary>
    /// Gets a <see cref="TimeOnly"/> representation of the value, if it can be converted.
    /// </summary>
    /// <returns></returns>
    Maybe<TimeOnly?> ToTimeOnly();
}

/// <summary>
/// Represents a Clarion type that can be interpreted as a date.
/// </summary>
public interface IClaDate : IClaObject
{
    /// <summary>
    /// Gets a <see cref="DateOnly"/> representation of the value, if it can be converted.
    /// </summary>
    /// <returns></returns>
    Maybe<DateOnly?> ToDateOnly();
}
