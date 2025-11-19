using System;
using System.Linq;
using TpsParser.TypeModel;

namespace TpsParser;

/// <summary>
/// Represents the schema for a particular field. For <c>MEMO</c>s and <c>BLOB</c>s, see <see cref="MemoDefinition"/>.
/// </summary>
public sealed record FieldDefinition
{
    /// <summary>
    /// Gets the data type.
    /// </summary>
    public FieldTypeCode TypeCode { get; init; }

    /// <summary>
    /// Gets the offset, in bytes, of the field within the record.
    /// </summary>
    public ushort Offset { get; init; }

    /// <summary>
    /// <para>
    /// Gets the fully qualified name of the field with the table prefix, e.g. "INV:INVOICENO".
    /// Use <see cref="Name"/> for only the field name.
    /// </para>
    /// <para>
    /// If the field was not defined with a prefix in the Clarion <c>FILE</c> declaration,
    /// then the prefix part will be absent.
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
    public ushort ElementCount { get; init; }

    /// <summary>
    /// Returns <see langword="true"/> if the field contains an array of values.
    /// </summary>
    public bool IsArray => ElementCount > 1;

    /// <summary>
    /// Gets the number of bytes in each element.
    /// </summary>
    public ushort Length { get; init; }

    /// <summary></summary>
    public ushort Flags { get; init; }

    /// <summary>
    /// Gets the index of the field in the record, starting from zero.
    /// </summary>
    public ushort Index { get; init; }

    /// <summary>
    /// If the field contains a <see cref="ClaFString"/>, <see cref="ClaCString"/>, or <see cref="ClaPString"/>, gets the number of bytes in the string.
    /// </summary>
    public ushort StringLength { get; init; }

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
    public byte BcdElementLength { get; init; }

    /// <summary>
    /// Creates a new <see cref="FieldDefinition"/> by parsing the data from the given <see cref="TpsRandomAccess"/> reader.
    /// </summary>
    public static FieldDefinition Parse(TpsRandomAccess rx)
    {
        ArgumentNullException.ThrowIfNull(rx);

        FieldTypeCode typeCode = (FieldTypeCode)rx.ReadByte();
        ushort offset = rx.ReadUnsignedShortLE();
        string fullName = rx.ReadZeroTerminatedString();
        ushort elementCount = rx.ReadUnsignedShortLE();
        ushort length = rx.ReadUnsignedShortLE();
        ushort flags = rx.ReadUnsignedShortLE();
        ushort index = rx.ReadUnsignedShortLE();

        byte bcdDigitsAfterDecimalPoint = 0;
        byte bcdElementLength = 0;

        ushort stringLength = 0;
        string stringMask = string.Empty;

        if (typeCode == FieldTypeCode.Decimal)
        {
            bcdDigitsAfterDecimalPoint = rx.ReadByte();
            bcdElementLength = rx.ReadByte();
        }
        else if (typeCode == FieldTypeCode.FString
            || typeCode == FieldTypeCode.CString
            || typeCode == FieldTypeCode.PString)
        {
            stringLength = rx.ReadUnsignedShortLE();
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
        group.Offset <= Offset
        && group.Offset + group.Length >= Offset + Length;
}
