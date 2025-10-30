using TpsParser.Binary;

namespace TpsParser.Tps.Header
{
    public sealed class DataHeader : Header
    {
        public int RecordNumber { get; }

        public DataHeader(TpsRandomAccess rx)
            : base(rx)
        {
            AssertIsType(0xF3);

            RecordNumber = rx.LongBE();
        }

        public override string ToString() =>
            $"DataHeader({TableNumber}, {RecordNumber})";
    }
}
