using TpsParser.Binary;

namespace TpsParser.Tps.Header
{
    public sealed class TableDefinitionHeader : Header
    {
        public int Block { get; }

        public TableDefinitionHeader(RandomAccess rx)
            : base(rx)
        {
            if (rx is null)
            {
                throw new System.ArgumentNullException(nameof(rx));
            }

            AssertIsType(0xFA);

            Block = rx.ShortLE();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override string ToString() =>
            $"TableDef({TableNumber}, {Block})";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
