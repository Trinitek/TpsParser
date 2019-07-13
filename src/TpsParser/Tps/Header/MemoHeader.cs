using TpsParser.Binary;

namespace TpsParser.Tps.Header
{
    public sealed class MemoHeader : Header
    {
        /// <summary>
        /// Gets the number of the <see cref="Record.DataRecord"/> that owns this memo.
        /// </summary>
        public int OwningRecord { get; }

        public int SequenceNumber { get; }

        /// <summary>
        /// Gets the index at which the memo appears in the record. Corresponds to the index number of <see cref="Record.TableDefinitionRecord.Memos"/>.
        /// </summary>
        public int MemoIndex { get; }

        public MemoHeader(RandomAccess rx)
            : base(rx)
        {
            AssertIsType(0xFC);

            OwningRecord = rx.LongBE();
            MemoIndex = rx.Byte();
            SequenceNumber = rx.ShortBE();
        }

        public override string ToString() =>
            $"Memo(Table={TableNumber}, Owner={OwningRecord}, Index={MemoIndex}, Sequence={SequenceNumber})";
    }
}
