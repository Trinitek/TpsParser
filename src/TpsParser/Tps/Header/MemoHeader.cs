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
        int SequenceNumber { get; }

        /// <summary>
        /// Gets the index at which the memo appears in the record. Corresponds to the index number of <see cref="ITableDefinitionRecord.Memos"/>.
        /// </summary>
        int MemoIndex { get; }
    }

    /// <inheritdoc/>
    internal sealed class MemoHeader : Header, IMemoHeader
    {
        /// <inheritdoc/>
        public int OwningRecord { get; }

        /// <inheritdoc/>
        public int SequenceNumber { get; }

        /// <inheritdoc/>
        public int MemoIndex { get; }

        /// <summary>
        /// Instantiates a new header.
        /// </summary>
        /// <param name="rx"></param>
        public MemoHeader(TpsReader rx)
            : base(rx)
        {
            AssertIsType(0xFC);

            OwningRecord = rx.LongBE();
            MemoIndex = rx.Byte();
            SequenceNumber = rx.ShortBE();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override string ToString() =>
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
            $"Memo(Table={TableNumber}, Owner={OwningRecord}, Index={MemoIndex}, Sequence={SequenceNumber})";
    }
}
