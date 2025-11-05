using System;
using System.Linq;
using TpsParser.TypeModel;

namespace TpsParser.Tps.Record;

/// <summary>
/// Represents the schema for a particular field. For <c>MEMO</c>s and <c>BLOB</c>s, see <see cref="MemoDefinition"/>.
/// </summary>
public sealed record FieldDefinition
{
    /// <summary>
    /// Gets the type code of the value contained within the field.
    /// </summary>
    public ClaTypeCode TypeCode { get; init; }

    /// <summary>
    /// Gets the offset, in bytes, of the field within the record.
    /// </summary>
    public int Offset { get; init; }

    /// <summary>
    /// <para>
    /// Gets the fully qualified name of the field with the table prefix, e.g. "INV:INVOICENO".
    /// Use <see cref="Name"/> for only the field name.
    /// </para>
    /// <para>
    /// If the field was not defined with a prefix in Clarion, then it will be absent.
    /// When present, it is rarely the same as the table name, if the table has a name at all.
    /// </para>
    /// </summary>
    public required string FullName { get; init; }

    /// <summary>
    /// <para>
    /// Gets the name of the field without the table prefix, e.g. "INVOICENO".
    /// Use <see cref="FullName"/> for the fully qualified field name.
    /// </para>
    /// </summary>
    public string Name => FullName.Split(':').Last();

    /// <summary>
    /// If the field is an array of continuous values, gets the number of elements in the array. Otherwise, 1.
    /// </summary>
    public int ElementCount { get; init; }

    /// <summary>
    /// True if the field contains an array of values.
    /// </summary>
    public bool IsArray => ElementCount > 1;

    /// <summary>
    /// Gets the number of number of bytes in each element.
    /// </summary>
    public int Length { get; init; }

    public short Flags { get; init; }

    /// <summary>
    /// Gets the index of the field in the record, starting from zero. This corresponds to the index of the associated value in <see cref="IDataRecord.Values"/>.
    /// </summary>
    public int Index { get; init; }

    /// <summary>
    /// If the field contains a <see cref="ClaFString"/>, <see cref="ClaCString"/>, or <see cref="ClaPString"/>, gets the number of bytes in the string.
    /// </summary>
    public int StringLength { get; init; }

    /// <summary>
    /// If the field contains a <see cref="ClaFString"/>, <see cref="ClaCString"/>, or <see cref="ClaPString"/>, gets the string mask.
    /// </summary>
    public required string StringMask { get; init; }

    /// <summary>
    /// If the field contains a <see cref="ClaDecimal"/>, gets the number of places after the decimal point.
    /// </summary>
    public byte BcdDigitsAfterDecimalPoint { get; init; }

    /// <summary>
    /// If the field contains a <see cref="ClaDecimal"/>, gets the number of decimal places.
    /// </summary>
    public int BcdElementLength { get; init; }

    /// <summary>
    /// Creates a new <see cref="FieldDefinition"/> by parsing the data from the given <see cref="TpsRandomAccess"/> reader.
    /// </summary>
    public static FieldDefinition Parse(TpsRandomAccess rx)
    {
        ArgumentNullException.ThrowIfNull(rx);

        var typeCode = (ClaTypeCode)rx.ReadByte();
        var offset = rx.ReadShortLE();
        var fullName = rx.ReadZeroTerminatedString();
        var elementCount = rx.ReadShortLE();
        var length = rx.ReadShortLE();
        var flags = rx.ReadShortLE();
        var index = rx.ReadShortLE();

        byte bcdDigitsAfterDecimalPoint = 0;
        int bcdElementLength = 0;

        int stringLength = 0;
        string stringMask = string.Empty;

        if (typeCode == ClaTypeCode.Decimal)
        {
            bcdDigitsAfterDecimalPoint = rx.ReadByte();
            bcdElementLength = rx.ReadByte();
        }
        else if (typeCode == ClaTypeCode.FString
            || typeCode == ClaTypeCode.CString
            || typeCode == ClaTypeCode.PString)
        {
            stringLength = rx.ReadShortLE();
            stringMask = rx.ReadZeroTerminatedString();

            if (stringMask.Length == 0)
            {
                rx.ReadByte(); // Consume one byte.
            }
        }

        return new FieldDefinition
        {
            TypeCode = typeCode,
            Offset = offset,
            FullName = fullName,
            ElementCount = elementCount,
            Length = length,
            Flags = flags,
            Index = index,
            StringLength = stringLength,
            StringMask = stringMask,
            BcdDigitsAfterDecimalPoint = bcdDigitsAfterDecimalPoint,
            BcdElementLength = bcdElementLength
        };
    }

    /// <summary>
    /// Checks to see if this field fits in the given group field.
    /// </summary>
    /// <param name="group">The group field to check.</param>
    /// <returns></returns>
    public bool IsInGroup(FieldDefinition group) =>
        (group.Offset <= Offset)
        && ((group.Offset + group.Length) >= (Offset + Length));
}
