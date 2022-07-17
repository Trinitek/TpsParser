using TpsParser.Binary;

namespace TpsParser.Tps.Header
{
    public sealed class IndexHeader : Header
    {
        public int IndexNumber { get; }

        public IndexHeader(TpsReader rx)
            : base(rx)
        {
            IndexNumber = TableType;
        }

        public override string ToString() =>
            $"IndexHeader({IndexNumber})";
    }
}
