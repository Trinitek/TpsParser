using System;
using System.Linq;
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

        /// <summary>
        /// Gets the number of bytes from the start of the record where the field value begins.
        /// </summary>
        ushort Offset { get; }

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

        /// <summary>
        /// If the field is an array, gets the number of elements in the array. See <see cref="IsArray"/>.
        /// </summary>
        ushort ElementCount { get; }

        /// <summary>
        /// Gets the size of the field in bytes.
        /// </summary>
        ushort Length { get; }

        /// <summary>
        /// Gets the index of the field in the record, starting from zero. This corresponds to the index of the associated value in <see cref="IDataRecord.Values"/>.
        /// </summary>
        ushort Index { get; }

        /// <summary>
        /// If the field contains a <see cref="TpsString"/>, <see cref="TpsCString"/>, or <see cref="TpsPString"/>, gets the number of bytes in the string.
        /// </summary>
        ushort StringLength { get; }

        /// <summary>
        /// If the field contains a <see cref="TpsString"/>, <see cref="TpsCString"/>, or <see cref="TpsPString"/>, gets the string mask.
        /// </summary>
        string StringMask { get; }

        /// <summary>
        /// If the field contains a <see cref="TpsDecimal"/>, gets the number of places after the decimal point.
        /// </summary>
        byte BcdDigitsAfterDecimalPoint { get; }

        /// <summary>
        /// If the field contains a <see cref="TpsDecimal"/>, gets the number of decimal places.
        /// </summary>
        byte BcdElementLength { get; }

        /// <summary>
        /// True if the field contains an array of values.
        /// </summary>
        bool IsArray { get; }

        /// <summary>
        /// Gets the group to which the field belongs, if any.
        /// </summary>
        IFieldDefinitionRecord OwnerGroup { get; }

        /// <summary>
        /// Checks to see if this field fits in the given group field.
        /// </summary>
        /// <param name="fieldDefinitionRecord">The group field to check.</param>
        /// <returns></returns>
        bool IsInGroup(IFieldDefinitionRecord fieldDefinitionRecord);
    }

    internal sealed class FieldDefinitionRecord : IFieldDefinitionRecord
    {
        public TpsTypeCode Type { get; }

        public ushort Offset { get; }

        public string FullName { get; }

        public string Name => FullName.Split(':').Last();

        public ushort ElementCount { get; }

        public ushort Length { get; }

        public ushort Flags { get; }

        public ushort Index { get; }

        public ushort StringLength { get; }

        public string StringMask { get; }

        public byte BcdDigitsAfterDecimalPoint { get; }

        public byte BcdElementLength { get; }

        public bool IsArray => ElementCount > 1;

        public IFieldDefinitionRecord OwnerGroup { get; }

        public FieldDefinitionRecord(TpsReader rx, IFieldDefinitionRecord ownerGroup)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            OwnerGroup = ownerGroup;

            Type = (TpsTypeCode)rx.ReadByte();
            Offset = rx.ReadUnsignedShortLE();
            FullName = rx.ReadZeroTerminatedString();
            ElementCount = rx.ReadUnsignedShortLE();
            Length = rx.ReadUnsignedShortLE();
            Flags = rx.ReadUnsignedShortLE();
            Index = rx.ReadUnsignedShortLE();

            switch (Type)
            {
                case TpsTypeCode.Decimal:
                    BcdDigitsAfterDecimalPoint = rx.ReadByte();
                    BcdElementLength = rx.ReadByte();
                    break;
                case TpsTypeCode.String:
                case TpsTypeCode.CString:
                case TpsTypeCode.PString:
                    StringLength = rx.ReadUnsignedShortLE();
                    StringMask = rx.ReadZeroTerminatedString();
                    if (StringMask.Length == 0)
                    {
                        rx.ReadByte();
                    }
                    break;
            }
        }
        
        public bool IsInGroup(IFieldDefinitionRecord group) =>
            (group.Offset <= Offset)
            && ((group.Offset + group.Length) >= (Offset + Length));

        public override string ToString() =>
            $"Field(#{Index},T:{Type},OFS:{Offset},LEN:{Length},{FullName},{ElementCount},{Flags})";
    }
}
