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
        public int MemoIndex { get; }

        public MemoHeader(RandomAccess rx)
            : base(rx)
        {
            AssertIsType(0xFC);

            OwningRecord = rx.LongBE();
            MemoIndex = rx.LongBE();
            SequenceNumber = rx.ShortBE();
        }

        public override string ToString() =>
            $"Memo({TableNumber}, {OwningRecord}, {MemoIndex}, {SequenceNumber})";
    }
}
