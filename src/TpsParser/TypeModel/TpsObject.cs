using System;
using System.Collections;
using System.Text;
using TpsParser.Binary;
using TpsParser.Tps;

namespace TpsParser.TypeModel;

/// <summary>
/// Represents a typed object within the TopSpeed file.
/// </summary>
public interface ITpsObject
{
    /// <summary>
    /// Gets the type code of the object.
    /// </summary>
    TpsTypeCode TypeCode { get; }
}

/// <summary>
/// Represents a simple Clarion type.
/// </summary>
public interface ISimple : ITpsObject
{
    /// <summary>
    /// Gets a <see cref="bool"/> representation of the value as governed by Clarion logic evaluation rules for the type.
    /// For all numeric types, this will be true for all non-zero values.
    /// For all string types, this will be true for strings that have a length greater than zero and do not consist entirely of "blank" padding characters (ASCII 0x20, i.e. space).
    /// </summary>
    /// <returns></returns>
    bool ToBoolean();
}

/// <summary>
/// Represents a numeric Clarion type.
/// </summary>
public interface INumeric : ISimple
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

internal static class TpsObject
{
    /// <summary>
    /// Builds a <see cref="ITpsObject"/> from the given binary reader and field definition information.
    /// </summary>
    /// <param name="rx">The binary reader.</param>
    /// <param name="encoding">The text encoding to use when reading string values.</param>
    /// <param name="enumerator">An enumerator for a collection of field definitions, the first being the field to parse, followed by the remainder of the definitions.
    /// The enumerator must have already been advanced to the first item with a call to <see cref="IEnumerator.MoveNext"/>.</param>
    /// <returns></returns>
    internal static ITpsObject ParseField(TpsRandomAccess rx, Encoding encoding, FieldDefinitionEnumerator enumerator)
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

    private static ITpsObject ParseScalarField(TpsRandomAccess rx, Encoding encoding, FieldDefinitionEnumerator enumerator) =>
        ParseScalarField(
            rx: rx,
            encoding: encoding,
            length: enumerator.Current?.Length ?? throw new ArgumentException("The current element is null.", nameof(enumerator)),
            enumerator: enumerator);

    internal static ITpsObject ParseScalarField(TpsRandomAccess rx, Encoding encoding, int length, FieldDefinitionEnumerator enumerator)
    {
        ArgumentNullException.ThrowIfNull(rx);
        ArgumentNullException.ThrowIfNull(encoding);
        ArgumentNullException.ThrowIfNull(enumerator);

        var current = enumerator.Current ?? throw new ArgumentException("The first item in the enumerator is null.", nameof(enumerator));

        switch (current.Type)
        {
            case TpsTypeCode.Byte:
                return rx.ReadTpsByte();
            case TpsTypeCode.Short:
                return rx.ReadTpsShort();
            case TpsTypeCode.UShort:
                return rx.ReadTpsUnsignedShort();
            case TpsTypeCode.Date:
                return rx.ReadTpsDate();
            case TpsTypeCode.Time:
                return rx.ReadTpsTime();
            case TpsTypeCode.Long:
                return rx.ReadTpsLong();
            case TpsTypeCode.ULong:
                return rx.ReadTpsUnsignedLong();
            case TpsTypeCode.SReal:
                return rx.ReadTpsFloat();
            case TpsTypeCode.Real:
                return rx.ReadTpsDouble();
            case TpsTypeCode.Decimal:
                return rx.ReadTpsDecimal(length, current.BcdDigitsAfterDecimalPoint);
            case TpsTypeCode.String:
                return rx.ReadTpsString(length, encoding);
            case TpsTypeCode.CString:
                return rx.ReadTpsCString(encoding);
            case TpsTypeCode.PString:
                return rx.ReadTpsPString(encoding);
            case TpsTypeCode.Group:
                return TpsGroup.BuildFromFieldDefinitions(rx, encoding, enumerator);
            default:
                throw new ArgumentException($"Unsupported type {current.Type} ({length})", nameof(enumerator));
        }
    }
}

/// <summary>
/// Represents a complex Clarion type that owns one or more instances of <see cref="ITpsObject"/>.
/// </summary>
public interface IComplex : ITpsObject
{ }

/// <summary>
/// Represents a Clarion string type.
/// </summary>
public interface IString : ISimple
{
    /// <summary>
    /// Gets the string value.
    /// </summary>
    string Value { get; }
}

/// <summary>
/// Represents a Clarion type that can be interpreted as a point in time.
/// </summary>
public interface ITime : ITpsObject
{
    /// <summary>
    /// Gets a <see cref="TimeSpan"/> representation of the value, if it can be converted.
    /// </summary>
    /// <returns></returns>
    Maybe<TimeSpan?> ToTimeSpan();
}

/// <summary>
/// Represents a Clarion type that can be interpreted as a date.
/// </summary>
public interface IDate : ITpsObject
{
    /// <summary>
    /// Gets a <see cref="DateTime"/> representation of the value, if it can be converted.
    /// </summary>
    /// <returns></returns>
    Maybe<DateTime?> ToDateTime();
}
