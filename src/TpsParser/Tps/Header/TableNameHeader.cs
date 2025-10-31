using TpsParser.Binary;

namespace TpsParser.Tps.Header;

/// <summary>
/// Represents a file structure that contains the name of a table.
/// </summary>
public interface ITableNameHeader
{
    /// <summary>
    /// Gets the name of the table.
    /// </summary>
    string Name { get; }
}

/// <summary>
/// Represents a file structure that contains the name of a table.
/// </summary>
public sealed class TableNameHeader : Header, ITableNameHeader
{
    /// <inheritdoc/>
    public string Name { get; }

    /// <summary>
    /// Instantiates a new header that describes the name of the table.
    /// </summary>
    /// <param name="rx"></param>
    public TableNameHeader(TpsRandomAccess rx)
        : base(rx, readTable: false)
    {
        AssertIsType(0xFE);

        Name = rx.ReadFixedLengthString(rx.Length - rx.Position);
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public override string ToString() =>
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        $"TableName({Name})";
}
