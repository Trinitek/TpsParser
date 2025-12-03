using TpsParser.TypeModel;

namespace TpsParser;

/// <summary>
/// A code point in a TopSpeed file that represents the data type for a <see cref="FieldDefinition"/>.
/// Member names are labeled after the corresponding Clarion language keyword.
/// </summary>
public enum FieldTypeCode : byte
{
    /// <summary>
    /// Represents a code point that is not defined in the file format.
    /// </summary>
    None = 0x00,

    /// <summary>
    /// Clarion <c>BYTE</c>. 1-byte unsigned integer. Modeled by <see cref="ClaByte"/>.
    /// </summary>
    Byte = 0x01,

    /// <summary>
    /// Clarion <c>SHORT</c>. 2-byte signed integer. Modeled by <see cref="ClaShort"/>.
    /// </summary>
    Short = 0x02,

    /// <summary>
    /// Clarion <c>USHORT</c>. 2-byte unsigned integer. Modeled by <see cref="ClaUnsignedShort"/>.
    /// </summary>
    UShort = 0x03,

    /// <summary>
    /// Clarion <c>DATE</c>. 4-byte date. Modeled by <see cref="ClaDate"/>.
    /// </summary>
    Date = 0x04,

    /// <summary>
    /// Clarion <c>TIME</c>. 4-byte time. Modeled by <see cref="ClaTime"/>.
    /// </summary>
    Time = 0x05,

    /// <summary>
    /// Clarion <c>LONG</c>. 4-byte signed integer. Modeled by <see cref="ClaLong"/>.
    /// </summary>
    Long = 0x06,

    /// <summary>
    /// Clarion <c>ULONG</c>. 4-byte unsigned integer. Modeled by <see cref="ClaUnsignedLong"/>.
    /// </summary>
    ULong = 0x07,

    /// <summary>
    /// Clarion <c>SREAL</c>. 4-byte signed floating point. Modeled by <see cref="ClaSingleReal"/>.
    /// </summary>
    SReal = 0x08,

    /// <summary>
    /// Clarion <c>REAL</c>. 8-byte signed floating point. Modeled by <see cref="ClaReal"/>.
    /// </summary>
    Real = 0x09,

    /// <summary>
    /// Clarion <c>DECIMAL</c>. Signed packed decimal. Modeled by <see cref="ClaDecimal"/>.
    /// </summary>
    Decimal = 0x0A,

    /// <summary>
    /// Clarion <c>STRING</c>. Fixed length string, padded with spaces. Modeled by <see cref="ClaFString"/>.
    /// </summary>
    FString = 0x12,

    /// <summary>
    /// Clarion <c>CSTRING</c>. Null terminated string. Modeled by <see cref="ClaCString"/>.
    /// </summary>
    CString = 0x13,

    /// <summary>
    /// Clarion <c>PSTRING</c>. Embedded length-byte "Pascal" string. Modeled by <see cref="ClaPString"/>.
    /// </summary>
    PString = 0x14,

    /// <summary>
    /// Clarion <c>GROUP</c>. Compound data structure. Modeled by <see cref="ClaGroup"/>.
    /// </summary>
    Group = 0x16
}
