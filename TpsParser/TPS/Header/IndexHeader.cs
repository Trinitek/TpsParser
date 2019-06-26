using TpsParser.Binary;

namespace TpsParser.TPS.Header
{
    public sealed class IndexHeader : Header
    {
        public int IndexNumber { get; }

        public IndexHeader(RandomAccess rx)
            : base(rx)
        {
            IndexNumber = TableType;
        }

        public override string ToString() =>
            $"IndexHeader({IndexNumber})";
    }
}
