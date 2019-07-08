using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TpsParser.Binary;
using TpsParser.Tps.Type;

namespace TpsParser.Tps.Record
{
    public sealed class TableDefinitionRecord
    {
        public int DriverVersion { get; }
        public int RecordLength { get; }
        private int FieldCount { get; }
        private int MemoCount { get; }
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

        public TpsObject ParseField(int type, int length, FieldDefinitionRecord fieldDefinitionRecord, RandomAccess rx)
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
                case 0x01:
                    AssertEqual(1, length);
                    return new TpsByte(rx);
                case 0x02:
                    AssertEqual(2, length);
                    return new TpsShort(rx);
                case 0x03:
                    AssertEqual(2, length);
                    return new TpsUnsignedShort(rx);
                case 0x04:
                    return new TpsDate(rx);
                case 0x05:
                    return new TpsTime(rx);
                case 0x06:
                    AssertEqual(4, length);
                    return new TpsLong(rx);
                case 0x07:
                    AssertEqual(4, length);
                    return new TpsUnsignedLong(rx);
                case 0x08:
                    AssertEqual(4, length);
                    return new TpsFloat(rx);
                case 0x09:
                    AssertEqual(8, length);
                    return new TpsDouble(rx);
                case 0x0A:
                    return new TpsDecimal(rx, length, fieldDefinitionRecord.BcdDigitsAfterDecimalPoint);
                case 0x12:
                    return new TpsString(rx, length, Encoding);
                case 0x13:
                    return new TpsCString(rx, Encoding);
                case 0x14:
                    return new TpsPString(rx, Encoding);
                case 0x16:
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
