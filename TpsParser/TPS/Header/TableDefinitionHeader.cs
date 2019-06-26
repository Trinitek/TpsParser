using TpsParser.Binary;

namespace TpsParser.Tps.Header
{
    public sealed class TableDefinitionHeader : Header
    {
        public int Block { get; }

        public TableDefinitionHeader(RandomAccess rx)
            : base(rx)
        {
            AssertIsType(0xFA);

            Block = rx.ShortLE();
        }

        public override string ToString() =>
            $"TableDef({TableNumber}, {Block})";
    }
}
