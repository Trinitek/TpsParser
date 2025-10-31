namespace TpsParser.Tps.Type;

/// <summary>
/// Represents a code point in a TopSpeed file that represents a particular data type.
/// </summary>
public enum TpsTypeCode : byte
{
    /// <summary>
    /// 1-byte unsigned integer
    /// </summary>
    Byte    = 0x01,

    /// <summary>
    /// 2-byte signed integer
    /// </summary>
    Short   = 0x02,

    /// <summary>
    /// 2-byte unsigned integer
    /// </summary>
    UShort  = 0x03,

    /// <summary>
    /// 4-byte date
    /// </summary>
    Date    = 0x04,

    /// <summary>
    /// 4-byte time
    /// </summary>
    Time    = 0x05,

    /// <summary>
    /// 4-byte signed integer
    /// </summary>
    Long    = 0x06,

    /// <summary>
    /// 4-byte unsigned integer
    /// </summary>
    ULong   = 0x07,

    /// <summary>
    /// 4-byte signed floating point
    /// </summary>
    SReal   = 0x08,

    /// <summary>
    /// 8-byte signed floating point
    /// </summary>
    Real    = 0x09,

    /// <summary>
    /// Signed packed decimal
    /// </summary>
    Decimal = 0x0A,

    /// <summary>
    /// Fixed length string, padded with spaces
    /// </summary>
    String  = 0x12,

    /// <summary>
    /// Null terminated string
    /// </summary>
    CString = 0x13,

    /// <summary>
    /// Embedded length-byte "Pascal" string
    /// </summary>
    PString = 0x14,

    /// <summary>
    /// Compound data structure
    /// </summary>
    Group   = 0x16,

    /// <summary>
    /// Variable-length binary large object
    /// </summary>
    Blob    = 0xFF,

    /// <summary>
    /// Fixed length string
    /// </summary>
    Memo    = 0xFF
}
