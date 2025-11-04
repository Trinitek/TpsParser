using System;
using System.Collections;
using System.Text;
using TpsParser.Tps;

namespace TpsParser.TypeModel;

/// <summary>
/// Represents a typed object within the TopSpeed file.
/// <see cref="IClaObject"/> types are modeled to correspond directly to types found in the Clarion runtime.
/// </summary>
public interface IClaObject
{
    /// <summary>
    /// Gets the type code of the object.
    /// </summary>
    ClaTypeCode TypeCode { get; }
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

internal static class ClaObject
{
    /// <summary>
    /// Builds a <see cref="IClaObject"/> from the given binary reader and field definition information.
    /// </summary>
    /// <param name="rx">The binary reader.</param>
    /// <param name="encoding">The text encoding to use when reading string values.</param>
    /// <param name="enumerator">An enumerator for a collection of field definitions, the first being the field to parse, followed by the remainder of the definitions.
    /// The enumerator must have already been advanced to the first item with a call to <see cref="IEnumerator.MoveNext"/>.</param>
    /// <returns></returns>
    internal static IClaObject ParseField(TpsRandomAccess rx, Encoding encoding, FieldDefinitionEnumerator enumerator)
    {
        ArgumentNullException.ThrowIfNull(rx);
        ArgumentNullException.ThrowIfNull(encoding);
        ArgumentNullException.ThrowIfNull(enumerator);

        var current = enumerator.Current ?? throw new ArgumentException("The first item in the enumerator is null.", nameof(enumerator));

        if (current.IsArray)
        {
            return TpsArrayExtensions.Parse(rx, encoding, enumerator);
        }
        else
        {
            return ParseScalarField(rx, encoding, enumerator);
        }
    }

    private static IClaObject ParseScalarField(TpsRandomAccess rx, Encoding encoding, FieldDefinitionEnumerator enumerator) =>
        ParseScalarField(
            rx: rx,
            encoding: encoding,
            length: enumerator.Current?.Length ?? throw new ArgumentException("The current element is null.", nameof(enumerator)),
            enumerator: enumerator);

    internal static IClaObject ParseScalarField(TpsRandomAccess rx, Encoding encoding, int length, FieldDefinitionEnumerator enumerator)
    {
        ArgumentNullException.ThrowIfNull(rx);
        ArgumentNullException.ThrowIfNull(encoding);
        ArgumentNullException.ThrowIfNull(enumerator);

        var current = enumerator.Current ?? throw new ArgumentException("The first item in the enumerator is null.", nameof(enumerator));

        switch (current.TypeCode)
        {
            case ClaTypeCode.Byte:
                return rx.ReadClaByte();
            case ClaTypeCode.Short:
                return rx.ReadClaShort();
            case ClaTypeCode.UShort:
                return rx.ReadClaUnsignedShort();
            case ClaTypeCode.Date:
                return rx.ReadClaDate();
            case ClaTypeCode.Time:
                return rx.ReadClaTime();
            case ClaTypeCode.Long:
                return rx.ReadClaLong();
            case ClaTypeCode.ULong:
                return rx.ReadClaUnsignedLong();
            case ClaTypeCode.SReal:
                return rx.ReadClaFloat();
            case ClaTypeCode.Real:
                return rx.ReadClaDouble();
            case ClaTypeCode.Decimal:
                return rx.ReadClaDecimal(length, current.BcdDigitsAfterDecimalPoint);
            case ClaTypeCode.FString:
                return rx.ReadClaFString(length, encoding);
            case ClaTypeCode.CString:
                return rx.ReadClaCString(encoding);
            case ClaTypeCode.PString:
                return rx.ReadClaPString(encoding);
            case ClaTypeCode.Group:
                return TpsGroup.BuildFromFieldDefinitions(rx, encoding, enumerator);
            default:
                throw new ArgumentException($"Unsupported type {current.TypeCode} ({length})", nameof(enumerator));
        }
    }
}

/// <summary>
/// Represents a complex Clarion type that owns one or more instances of <see cref="IClaObject"/>.
/// </summary>
public interface IComplex : IClaObject
{ }

/// <summary>
/// Represents a Clarion string type.
/// </summary>
public interface IClaString : IClaBoolean
{
    /// <summary>
    /// Gets the string value.
    /// </summary>
    string Value { get; }
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
