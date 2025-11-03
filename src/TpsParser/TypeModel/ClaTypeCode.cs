namespace TpsParser.TypeModel;

/// <summary>
/// Represents a code point in a TopSpeed file that represents a particular data type.
/// </summary>
public enum ClaTypeCode : byte
{
    /// <summary>
    /// Represents a code point that is not defined in the file format.
    /// </summary>
    None    = 0x00,

    /// <summary>
    /// Clarion BYTE. 1-byte unsigned integer.
    /// </summary>
    Byte    = 0x01,

    /// <summary>
    /// Clarion SHORT. 2-byte signed integer.
    /// </summary>
    Short   = 0x02,

    /// <summary>
    /// Clarion USHORT. 2-byte unsigned integer.
    /// </summary>
    UShort  = 0x03,

    /// <summary>
    /// Clarion DATE. 4-byte date.
    /// </summary>
    Date    = 0x04,

    /// <summary>
    /// Clarion TIME. 4-byte time.
    /// </summary>
    Time    = 0x05,

    /// <summary>
    /// Clarion LONG. 4-byte signed integer.
    /// </summary>
    Long    = 0x06,

    /// <summary>
    /// Clarion ULONG. 4-byte unsigned integer.
    /// </summary>
    ULong   = 0x07,

    /// <summary>
    /// Clarion SREAL. 4-byte signed floating point.
    /// </summary>
    SReal   = 0x08,

    /// <summary>
    /// Clarion REAL. 8-byte signed floating point.
    /// </summary>
    Real    = 0x09,

    /// <summary>
    /// Clarion DECIMAL. Signed packed decimal.
    /// </summary>
    Decimal = 0x0A,

    /// <summary>
    /// Clarion STRING. Fixed length string, padded with spaces.
    /// </summary>
    FString  = 0x12,

    /// <summary>
    /// Clarion CSTRING. Null terminated string.
    /// </summary>
    CString = 0x13,

    /// <summary>
    /// Clarion PSTRING. Embedded length-byte "Pascal" string.
    /// </summary>
    PString = 0x14,

    /// <summary>
    /// Clarion GROUP. Compound data structure.
    /// </summary>
    Group   = 0x16
}
