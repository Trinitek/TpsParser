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
    public sealed class TableDefinitionRecord
    {
        public int DriverVersion { get; }
        public int RecordLength { get; }

        /// <summary>
        /// Gets the number of fields in the table.  For MEMOs and BLOBs, see <see cref="MemoCount"/>.
        /// </summary>
        private int FieldCount { get; }

        /// <summary>
        /// Gets the number of MEMO or BLOB fields in the table.
        /// </summary>
        private int MemoCount { get; }

        /// <summary>
        /// Gets the number of indexes in the table.
        /// </summary>
        private int IndexCount { get; }

        public IReadOnlyList<FieldDefinitionRecord> Fields => _fields;
        private readonly List<FieldDefinitionRecord> _fields;
        public IReadOnlyList<MemoDefinitionRecord> Memos => _memos;
        private readonly List<MemoDefinitionRecord> _memos;
        public IReadOnlyList<IndexDefinitionRecord> Indexes => _indexes;
        private readonly List<IndexDefinitionRecord> _indexes;

        private Encoding Encoding { get; }

        public TableDefinitionRecord(RandomAccess rx, Encoding encoding)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

            DriverVersion = rx.ShortLE();
            RecordLength = rx.ShortLE();
            FieldCount = rx.ShortLE();
            MemoCount = rx.ShortLE();
            IndexCount = rx.ShortLE();

            _fields = new List<FieldDefinitionRecord>();
            _memos = new List<MemoDefinitionRecord>();
            _indexes = new List<IndexDefinitionRecord>();

            try
            {
                for (int i = 0; i < FieldCount; i++)
                {
                    _fields.Add(new FieldDefinitionRecord(rx));
                }
                for (int i = 0; i < MemoCount; i++)
                {
                    _memos.Add(new MemoDefinitionRecord(rx));
                }
                for (int i = 0; i < IndexCount; i++)
                {
                    _indexes.Add(new IndexDefinitionRecord(rx));
                }
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"Bad table definition: {ToString()}", ex);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"TableDefinition({DriverVersion},{RecordLength},{FieldCount},{MemoCount},{IndexCount}");

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

        public IEnumerable<TpsObject> Parse(byte[] record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            var rx = new RandomAccess(record);
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

            return values;
        }

        public TpsObject ParseField(TpsTypeCode type, int length, FieldDefinitionRecord fieldDefinitionRecord, RandomAccess rx)
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
                case TpsTypeCode.BFloat4:
                    AssertEqual(4, length);
                    return new TpsFloat(rx);
                case TpsTypeCode.BFloat8:
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
