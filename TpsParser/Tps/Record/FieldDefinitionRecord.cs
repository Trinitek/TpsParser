using System;
using TpsParser.Binary;

namespace TpsParser.Tps.Record
{
    public sealed class FieldDefinitionRecord
    {
        public int FieldType { get; }
        public int Offset { get; }
        public string FieldName { get; }
        public int ElementCount { get; }
        public int Length { get; }
        private int Flags { get; }
        private int Index { get; }

        public int StringLength { get; }
        public string StringMask { get; }

        public int BcdDigitsAfterDecimalPoint { get; }
        public int BcdElementLength { get; }

        public bool IsArray => ElementCount > 1;
        public bool IsGroup => FieldType == 0x16;

        public FieldDefinitionRecord(RandomAccess rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            FieldType = rx.Byte();
            Offset = rx.ShortLE();
            FieldName = rx.ZeroTerminatedString();
            ElementCount = rx.ShortLE();
            Length = rx.ShortLE();
            Flags = rx.ShortLE();
            Index = rx.ShortLE();

            switch (FieldType)
            {
                case 0x0A:
                    BcdDigitsAfterDecimalPoint = rx.Byte();
                    BcdElementLength = rx.Byte();
                    break;
                case 0x12:
                case 0x13:
                case 0x14:
                    StringLength = rx.ShortLE();
                    StringMask = rx.ZeroTerminatedString();
                    if (StringMask.Length == 0)
                    {
                        rx.Byte();
                    }
                    break;
            }
        }

        public string FieldTypeName()
        {
            switch (FieldType)
            {
                case 0x01:
                    return "BYTE";
                case 0x02:
                    return "SIGNED-SHORT";
                case 0x03:
                    return "UNSIGNED-SHORT";
                case 0x04:
                    return "DATE";
                case 0x05:
                    return "TIME";
                case 0x06:
                    return "SIGNED-LONG";
                case 0x07:
                    return "UNSIGNED-LONG";
                case 0x08:
                    return "Float";
                case 0x09:
                    return "Double";
                case 0x0A:
                    return "BCD";
                case 0x12:
                    return "fixed-length STRING";
                case 0x13:
                    return "zero-terminated STRING";
                case 0x14:
                    return "pascal STRING";
                case 0x16:
                    return "GROUP";
                default:
                    return "unknown";
            }
        }

        /// <summary>
        /// Checks to see if this field fits in the given group field.
        /// </summary>
        /// <param name="group">The group field to check.</param>
        /// <returns></returns>
        public bool IsInGroup(FieldDefinitionRecord group) =>
            (group.Offset <= Offset)
            && ((group.Offset + group.Length) >= (Offset + Length));
    }
}
