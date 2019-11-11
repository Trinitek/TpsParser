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
        int DriverVersion { get; }

        /// <summary>
        /// Gets the total number of bytes in a table record.  This is equivalent to the sum of the field lengths in <see cref="Fields"/>.
        /// This does not count MEMO or BLOB sizes as those are stored in a separate file structure.
        /// </summary>
        /// <remarks>
        /// The length property on GROUP fields count the total length of all of its child fields.
        /// When a GROUP field is present, the length is only counted once to reflect the number of bytes actually reserved for storage in the record.
        /// </remarks>
        int RecordLength { get; }

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

        public IReadOnlyList<IFieldDefinitionRecord> Fields => _fields;
        private readonly List<FieldDefinitionRecord> _fields;

        public IReadOnlyList<IMemoDefinitionRecord> Memos => _memos;
        private readonly List<MemoDefinitionRecord> _memos;

        public IReadOnlyList<IIndexDefinitionRecord> Indexes => _indexes;
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
            int fieldCount = rx.ShortLE();
            int memoCount = rx.ShortLE();
            int indexCount = rx.ShortLE();

            _fields = new List<FieldDefinitionRecord>();
            _memos = new List<MemoDefinitionRecord>();
            _indexes = new List<IndexDefinitionRecord>();

            var groups = new Stack<FieldDefinitionRecord>();

            try
            {
                for (int i = 0; i < fieldCount; i++)
                {
                    if (groups.Any() && _fields.Any())
                    {
                        var lastGroup = groups.Peek();
                        var lastField = _fields.Last();

                        if (lastGroup.Offset + lastGroup.Length <= lastField.Offset + lastField.Length
                            && lastGroup != lastField)
                        {
                            groups.Pop();
                        }
                    }

                    var field = new FieldDefinitionRecord(rx, groups.FirstOrDefault());

                    if (field.Type == TpsTypeCode.Group)
                    {
                        groups.Push(field);
                    }

                    _fields.Add(field);
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
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        public IReadOnlyList<TpsObject> Parse(byte[] record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            var rx = new RandomAccess(record);
            var values = new List<TpsObject>(Fields.Count);

            using (var fieldEnumerator = new FieldDefinitionEnumerator(Fields))
            {
                while (fieldEnumerator.MoveNext())
                {
                    Console.WriteLine(fieldEnumerator.Current);

                    values.Add(TpsObject.ParseField(rx, Encoding, fieldEnumerator));
                }
            }

            return values.AsReadOnly();
        }
    }
}
