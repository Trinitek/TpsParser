using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TpsParser.Binary;

namespace TpsParser.Tps.Record
{
    public sealed class TableDefinitionRecord
    {
        public int DriverVersion { get; }
        public int RecordLength { get; }
        private int FieldCount { get; }
        private int MemoCount { get; }
        private int IndexCount { get; }

        public IEnumerable<FieldDefinitionRecord> Fields => _fields;
        private readonly List<FieldDefinitionRecord> _fields;
        public IEnumerable<MemoDefinitionRecord> Memos => _memos;
        private readonly List<MemoDefinitionRecord> _memos;
        public IEnumerable<IndexDefinitionRecord> Indexes => _indexes;
        private readonly List<IndexDefinitionRecord> _indexes;

        private Encoding Encoding { get; }

        public TableDefinitionRecord(RandomAccess rx, Encoding encoding)
        {
            if (rx == null)
            {
                throw new System.ArgumentNullException(nameof(rx));
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
                for (int i = 0; i < FieldCount; i++)
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
                sb.AppendLine(field.ToString());
            }
            foreach (var memo in Memos)
            {
                sb.AppendLine(memo.ToString());
            }
            foreach (var index in Indexes)
            {
                sb.AppendLine(index.ToString());
            }

            sb.Append(")");

            return sb.ToString();
        }

        public IEnumerable<object> Parse(byte[] record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            var rx = new RandomAccess(record);
            var values = new List<object>(Fields.Count());

            foreach (var field in Fields)
            {
                if (field.IsArray)
                {
                    int fieldSize = RecordLength / field.ElementCount;

                    for (int i = 0; i < field.ElementCount; i++)
                    {
                        values.Add(ParseField(field.FieldType, fieldSize, field, rx));
                    }
                }
                else
                {
                    values.Add(ParseField(field.FieldType, field.Length, field, rx));
                }
            }

            return values;
        }

        public object ParseField(int type, int length, FieldDefinitionRecord fieldDefinitionRecord, RandomAccess rx)
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
                    return rx.Byte();
                case 0x02:
                    AssertEqual(2, length);
                    return rx.ShortLE();
                case 0x03:
                    AssertEqual(2, length);
                    return rx.UnsignedShortLE();
                case 0x04:
                    // Date, mask encoded
                    long date = rx.UnsignedLongLE();
                    if (date != 0)
                    {
                        long years = (date & 0xFFFF0000) >> 16;
                        long months = (date & 0x0000FF00) >> 8;
                        long days = date & 0x000000FF;
                        return new DateTime((int)years, (int)months, (int)days);
                    }
                    else
                    {
                        return null;
                    }
                case 0x05:
                    // Time, mask encoded
                    // Currently only knows how to handle hours and minutes, but not seconds or milliseconds.
                    int time = rx.LongLE();
                    int mins = (time & 0x00FF0000) >> 16;
                    int hours = (time & 0x7F000000) >> 24;
                    return new TimeSpan(hours, mins, 0);
                case 0x06:
                    AssertEqual(4, length);
                    return rx.LongLE();
                case 0x07:
                    AssertEqual(4, length);
                    return rx.UnsignedLongLE();
                case 0x08:
                    AssertEqual(4, length);
                    return rx.FloatLE();
                case 0x09:
                    AssertEqual(8, length);
                    return rx.DoubleLE();
                case 0x0A:
                    return rx.BinaryCodedDecimal(length, fieldDefinitionRecord.BcdDigitsAfterDecimalPoint);
                case 0x12:
                    return rx.FixedLengthString(length, Encoding);
                case 0x13:
                    return rx.ZeroTerminatedString(Encoding);
                case 0x14:
                    return rx.PascalString(Encoding);
                case 0x16:
                    // Group (an overlay on top of existing data, can be anything)
                    return rx.ReadBytes(length);
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
