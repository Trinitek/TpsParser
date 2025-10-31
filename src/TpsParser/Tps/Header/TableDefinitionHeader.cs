using TpsParser.Binary;

namespace TpsParser.Tps.Header;

public sealed class TableDefinitionHeader : Header
{
    public int Block { get; }

    public TableDefinitionHeader(TpsRandomAccess rx)
        : base(rx)
    {
        AssertIsType(0xFA);

        Block = rx.ReadShortLE();
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"TableDef({TableNumber}, {Block})";
}
