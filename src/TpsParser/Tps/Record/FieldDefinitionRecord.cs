using System;
using System.Linq;
using TpsParser.Binary;
using TpsParser.Tps.Type;

namespace TpsParser.Tps.Record
{
    /// <summary>
    /// Represents the schema for a particular field. For MEMOs and BLOBs, see <see cref="IMemoDefinitionRecord"/>.
    /// </summary>
    public interface IFieldDefinitionRecord
    {
        /// <summary>
        /// Gets the type code of the value contained within the field.
        /// </summary>
        TpsTypeCode Type { get; }

        int Offset { get; }

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
        string FullName { get; }

        /// <summary>
        /// <para>
        /// Gets the name of the field without the table prefix, e.g. "INVOICENO".
        /// Use <see cref="FullName"/> for the fully qualified field name.
        /// </para>
        /// </summary>
        string Name { get; }

        int ElementCount { get; }

        int Length { get; }

        int Index { get; }

        int StringLength { get; }

        string StringMask { get; }

        int BcdDigitsAfterDecimalPoint { get; }

        int BcdElementLength { get; }

        bool IsArray { get; }
    }

    /// <summary>
    /// Represents the schema for a particular field. For MEMOs and BLOBs, see <see cref="MemoDefinitionRecord"/>.
    /// </summary>
    public sealed class FieldDefinitionRecord : IFieldDefinitionRecord
    {
        /// <inheritdoc/>
        public TpsTypeCode Type { get; }

        /// <inheritdoc/>
        public int Offset { get; }

        /// <inheritdoc/>
        public string FullName { get; }

        /// <inheritdoc/>
        public string Name => FullName.Split(':').Last();

        /// <inheritdoc/>
        public int ElementCount { get; }

        /// <inheritdoc/>
        public int Length { get; }

        /// <inheritdoc/>
        public int Flags { get; }

        /// <inheritdoc/>
        public int Index { get; }

        /// <inheritdoc/>
        public int StringLength { get; }

        /// <inheritdoc/>
        public string StringMask { get; }

        /// <inheritdoc/>
        public int BcdDigitsAfterDecimalPoint { get; }

        /// <inheritdoc/>
        public int BcdElementLength { get; }

        /// <inheritdoc/>
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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override string ToString() =>
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
            $"Field(#{Index},T:{Type},OFS:{Offset},LEN:{Length},{FullName},{ElementCount},{Flags})";
    }
}
