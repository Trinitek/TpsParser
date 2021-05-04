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

        /// <inheritdoc/>
        public override string ToString() =>
            $"IndexHeader({IndexNumber})";
    }
}
