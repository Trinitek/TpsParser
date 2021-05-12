using System;
using TpsParser.Tps.Record;

namespace TpsParser.Tps.Header
{
    /// <summary>
    /// Encapsulates information about a particular <see cref="IMemoRecord"/>.
    /// </summary>
    public interface IMemoHeader : IHeader
    {
        /// <summary>
        /// Gets the number of the <see cref="IDataRecord"/> that owns this memo.
        /// </summary>
        int OwningRecord { get; }

        /// <summary>
        /// Gets the sequence number of the memo when the memo is segmented into multiple records. The first segment is 0.
        /// </summary>
        short SequenceNumber { get; }

        /// <summary>
        /// Gets the index at which the memo appears in the record. Corresponds to the index number of <see cref="ITableDefinitionRecord.Memos"/>.
        /// </summary>
        byte MemoIndex { get; }
    }

    /// <inheritdoc/>
    public sealed class MemoHeader : HeaderBase, IMemoHeader
    {
        /// <inheritdoc/>
        public int OwningRecord { get; }

        /// <inheritdoc/>
        public short SequenceNumber { get; }

        /// <inheritdoc/>
        public byte MemoIndex { get; }

        /// <summary>
        /// Instantiates a new header.
        /// </summary>
        /// <param name="tableNumber"></param>
        /// <param name="kind"></param>
        /// <param name="owningRecord"></param>
        /// <param name="sequenceNumber"></param>
        /// <param name="memoIndex"></param>
        public MemoHeader(int tableNumber, HeaderKind kind, int owningRecord, short sequenceNumber, byte memoIndex)
            : base(tableNumber, kind)
        {
            AssertIsType(HeaderKind.Memo);

            OwningRecord = owningRecord;
            SequenceNumber = sequenceNumber;
            MemoIndex = memoIndex;
        }

        /// <inheritdoc/>
        public override string ToString() =>
            $"Memo(Table={TableNumber}, Owner={OwningRecord}, Index={MemoIndex}, Sequence={SequenceNumber})";

        /// <summary>
        /// Creates a new <see cref="MemoHeader"/> from the given reader.
        /// </summary>
        /// <param name="rx"></param>
        /// <returns></returns>
        public static MemoHeader Read(TpsReader rx)
        {
            if (rx is null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            return new MemoHeader(
                tableNumber: rx.ReadLongBE(),
                kind: (HeaderKind)rx.ReadByte(),
                owningRecord: rx.ReadLongBE(),
                memoIndex: rx.ReadByte(),
                sequenceNumber: rx.ReadShortBE());
        }
    }
}
