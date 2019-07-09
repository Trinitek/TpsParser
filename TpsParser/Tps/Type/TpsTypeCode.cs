namespace TpsParser.Tps.Type
{
    public enum TpsTypeCode : byte
    {
        Byte    = 0x01,
        Short   = 0x02,
        UShort  = 0x03,
        Date    = 0x04,
        Time    = 0x05,
        Long    = 0x06,
        ULong   = 0x07,
        BFloat4 = 0x08,
        BFloat8 = 0x09,
        Decimal = 0x0A,
        String  = 0x12,
        CString = 0x13,
        PString = 0x14,
        Group   = 0x16,

        Blob    = 0xFF,
        Memo    = 0xFF
    }
}
