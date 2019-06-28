using System;
using System.Linq;
using TpsParser.Binary;

namespace TpsParser.Tps.Record
{
    public sealed class FieldDefinitionRecord
    {
        public int Type { get; }
        public int Offset { get; }

        /// <summary>
        /// Gets the fully qualified name of the field with the table prefix, e.g. "INV:INVOICENO".
        /// Use <see cref="Name"/> for only the field name.
        /// </summary>
        public string FullName { get; }

        /// <summary>
        /// Gets the name of the field without the table prefix, e.g. "INVOICENO".
        /// Use <see cref="FullName"/> for the fully qualified field name.
        /// </summary>
        public string Name => FullName.Split(':').Last();

        public int ElementCount { get; }
        public int Length { get; }
        public int Flags { get; }
        public int Index { get; }

        public int StringLength { get; }
        public string StringMask { get; }

        public int BcdDigitsAfterDecimalPoint { get; }
        public int BcdElementLength { get; }

        public bool IsArray => ElementCount > 1;
        public bool IsGroup => Type == 0x16;

        public FieldDefinitionRecord(RandomAccess rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Type = rx.Byte();
            Offset = rx.ShortLE();
            FullName = rx.ZeroTerminatedString();
            ElementCount = rx.ShortLE();
            Length = rx.ShortLE();
            Flags = rx.ShortLE();
            Index = rx.ShortLE();

            switch (Type)
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

        /// <summary>
        /// Gets a string representation of the data type.
        /// </summary>
        public string TypeName
        {
            get
            {
                switch (Type)
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
