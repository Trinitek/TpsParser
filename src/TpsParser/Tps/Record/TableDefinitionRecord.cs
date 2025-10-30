using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TpsParser.Binary;
using TpsParser.Tps.Type;

namespace TpsParser.Tps.Record
{
    /// <summary>
    /// Represents a file structure that encapsulates a table's schema.
    /// </summary>
    public interface ITableDefinitionRecord
    {
        /// <summary>
        /// Gets the field definitions for this table.  For MEMOs and BLOBs, see <see cref="Memos"/>.
        /// </summary>
        IReadOnlyList<IFieldDefinitionRecord> Fields { get; }

        /// <summary>
        /// Gets the memo definitions for this table.  The index of each definition corresponds to <see cref="Header.IMemoHeader.MemoIndex"/>.
        /// </summary>
        IReadOnlyList<IMemoDefinitionRecord> Memos { get; }

        /// <summary>
        /// Gets the index definitions for this table.
        /// </summary>
        IReadOnlyList<IIndexDefinitionRecord> Indexes { get; }

        /// <summary>
        /// Gets a list of field values by parsing the given byte stream.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        IReadOnlyList<TpsObject> Parse(byte[] record);
    }

    /// <summary>
    /// Represents a file structure that encapsulates a table's schema.
    /// </summary>
    internal sealed class TableDefinitionRecord : ITableDefinitionRecord
    {
        public int DriverVersion { get; }
        public int RecordLength { get; }

        /// <inheritdoc/>
        public IReadOnlyList<IFieldDefinitionRecord> Fields => _fields;
        private readonly List<FieldDefinitionRecord> _fields;

        /// <inheritdoc/>
        public IReadOnlyList<IMemoDefinitionRecord> Memos => _memos;
        private readonly List<MemoDefinitionRecord> _memos;

        /// <inheritdoc/>
        public IReadOnlyList<IIndexDefinitionRecord> Indexes => _indexes;
        private readonly List<IndexDefinitionRecord> _indexes;

        private Encoding Encoding { get; }

        public TableDefinitionRecord(TpsRandomAccess rx, Encoding encoding)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

            DriverVersion = rx.ShortLE();
            RecordLength = rx.ShortLE();
            int fieldCount = rx.ShortLE();
            int memoCount = rx.ShortLE();
            int indexCount = rx.ShortLE();

            _fields = new List<FieldDefinitionRecord>();
            _memos = new List<MemoDefinitionRecord>();
            _indexes = new List<IndexDefinitionRecord>();

            try
            {
                for (int i = 0; i < fieldCount; i++)
                {
                    _fields.Add(new FieldDefinitionRecord(rx));
                }
                for (int i = 0; i < memoCount; i++)
                {
                    _memos.Add(new MemoDefinitionRecord(rx));
                }
                for (int i = 0; i < indexCount; i++)
                {
                    _indexes.Add(new IndexDefinitionRecord(rx));
                }
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"Bad table definition: {ToString()}", ex);
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override string ToString()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            var sb = new StringBuilder();

            sb.AppendLine($"TableDefinition({DriverVersion},{RecordLength},{Fields.Count},{Memos.Count},{Indexes.Count}");

            foreach (var field in Fields)
            {
                sb.AppendLine($"  {field.ToString()}");
            }
            foreach (var memo in Memos)
            {
                sb.AppendLine($"  {memo.ToString()}");
            }
            foreach (var index in Indexes)
            {
                sb.AppendLine($"  {index.ToString()}");
            }

            sb.Append(")");

            return sb.ToString();
        }

        public IReadOnlyList<TpsObject> Parse(byte[] record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            var rx = new TpsRandomAccess(record);
            var values = new List<TpsObject>(Fields.Count());

            foreach (var field in Fields)
            {
                if (field.IsArray)
                {
                    int fieldSize = RecordLength / field.ElementCount;

                    for (int i = 0; i < field.ElementCount; i++)
                    {
                        values.Add(ParseField(field.Type, fieldSize, field, rx));
                    }
                }
                else
                {
                    values.Add(ParseField(field.Type, field.Length, field, rx));
                }
            }

            return values.AsReadOnly();
        }

        private TpsObject ParseField(TpsTypeCode type, int length, IFieldDefinitionRecord fieldDefinitionRecord, TpsRandomAccess rx)
        {
            if (fieldDefinitionRecord == null)
            {
                throw new ArgumentNullException(nameof(fieldDefinitionRecord));
            }

            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            switch (type)
            {
                case TpsTypeCode.Byte:
                    AssertEqual(1, length);
                    return new TpsByte(rx);
                case TpsTypeCode.Short:
                    AssertEqual(2, length);
                    return new TpsShort(rx);
                case TpsTypeCode.UShort:
                    AssertEqual(2, length);
                    return new TpsUnsignedShort(rx);
                case TpsTypeCode.Date:
                    return new TpsDate(rx);
                case TpsTypeCode.Time:
                    return new TpsTime(rx);
                case TpsTypeCode.Long:
                    AssertEqual(4, length);
                    return new TpsLong(rx);
                case TpsTypeCode.ULong:
                    AssertEqual(4, length);
                    return new TpsUnsignedLong(rx);
                case TpsTypeCode.SReal:
                    AssertEqual(4, length);
                    return new TpsFloat(rx);
                case TpsTypeCode.Real:
                    AssertEqual(8, length);
                    return new TpsDouble(rx);
                case TpsTypeCode.Decimal:
                    return new TpsDecimal(rx, length, fieldDefinitionRecord.BcdDigitsAfterDecimalPoint);
                case TpsTypeCode.String:
                    return new TpsString(rx, length, Encoding);
                case TpsTypeCode.CString:
                    return new TpsCString(rx, Encoding);
                case TpsTypeCode.PString:
                    return new TpsPString(rx, Encoding);
                case TpsTypeCode.Group:
                    return new TpsGroup(rx, length);
                default:
                    throw new ArgumentException($"Unsupported type {type} ({length})", nameof(type));
            }
        }

        private void AssertEqual(int reference, int value)
        {
            if (reference != value)
            {
                throw new ArgumentException($"{reference} != {value}");
            }
        }
    }
}
