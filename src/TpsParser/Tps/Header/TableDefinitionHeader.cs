namespace TpsParser.Tps.Header
{
    public sealed class TableDefinitionHeader : Header
    {
        public int Block { get; }

        public TableDefinitionHeader(TpsReader rx)
            : base(rx)
        {
            if (rx is null)
            {
                throw new System.ArgumentNullException(nameof(rx));
            }

            AssertIsType(0xFA);

            Block = rx.ReadShortLE();
        }

        /// <inheritdoc/>
        public override string ToString() =>
            $"TableDef({TableNumber}, {Block})";
    }
}
