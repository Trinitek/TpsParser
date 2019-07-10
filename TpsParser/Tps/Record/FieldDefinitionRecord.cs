using System;
using System.Linq;
using TpsParser.Binary;
using TpsParser.Tps.Type;

namespace TpsParser.Tps.Record
{
    /// <summary>
    /// Represents the schema for a particular field. For MEMOs and BLOBs, see <see cref="MemoDefinitionRecord"/>.
    /// </summary>
    public sealed class FieldDefinitionRecord
    {
        public TpsTypeCode Type { get; }
        public int Offset { get; }

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
        public string FullName { get; }

        /// <summary>
        /// <para>
        /// Gets the name of the field without the table prefix, e.g. "INVOICENO".
        /// Use <see cref="FullName"/> for the fully qualified field name.
        /// </para>
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

        public FieldDefinitionRecord(RandomAccess rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Type = (TpsTypeCode)rx.Byte();
            Offset = rx.ShortLE();
            FullName = rx.ZeroTerminatedString();
            ElementCount = rx.ShortLE();
            Length = rx.ShortLE();
            Flags = rx.ShortLE();
            Index = rx.ShortLE();

            switch (Type)
            {
                case TpsTypeCode.Decimal:
                    BcdDigitsAfterDecimalPoint = rx.Byte();
                    BcdElementLength = rx.Byte();
                    break;
                case TpsTypeCode.String:
                case TpsTypeCode.CString:
                case TpsTypeCode.PString:
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
        /// Checks to see if this field fits in the given group field.
        /// </summary>
        /// <param name="group">The group field to check.</param>
        /// <returns></returns>
        public bool IsInGroup(FieldDefinitionRecord group) =>
            (group.Offset <= Offset)
            && ((group.Offset + group.Length) >= (Offset + Length));

        public override string ToString() =>
            $"Field(#{Index},T:{Type},OFS:{Offset},LEN:{Length},{FullName},{ElementCount},{Flags})";
    }
}
