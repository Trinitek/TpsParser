namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a code point in a TopSpeed file that represents a particular data type.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "The identifier names reflect the type names found in the Clarion language.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1028:Enum Storage should be Int32", Justification = "The corresponding data type in the TopSpeed file format is a byte.")]
    public enum TpsTypeCode : byte
    {
        /// <summary>
        /// Represents a code point that is not defined in the file format.
        /// </summary>
        None    = 0x00,

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
        Group   = 0x16
    }
}
