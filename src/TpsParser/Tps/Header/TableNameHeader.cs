using TpsParser.Binary;

namespace TpsParser.Tps.Header
{
    public sealed class TableNameHeader : Header
    {
        public string Name { get; }

        public TableNameHeader(RandomAccess rx)
            : base(rx, readTable: false)
        {
            AssertIsType(0xFE);

            Name = rx.FixedLengthString(rx.Length - rx.Position);
        }

        public override string ToString() =>
            $"TableName({Name})";
    }
}
