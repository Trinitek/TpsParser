using TpsParser.Binary;

namespace TpsParser.Tps.Header
{
    public sealed class IndexHeader : Header
    {
        public int IndexNumber { get; }

        public IndexHeader(TpsRandomAccess rx)
            : base(rx)
        {
            IndexNumber = TableType;
        }

        public override string ToString() =>
            $"IndexHeader({IndexNumber})";
    }
}
