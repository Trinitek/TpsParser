namespace TpsParser.Tps.Header
{
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
        public TableNameHeader(TpsReader rx)
            : base(rx, readTable: false)
        {
            AssertIsType(0xFE);

            Name = rx.FixedLengthString(rx.Length - rx.Position);
        }

        /// <inheritdoc/>
        public override string ToString() =>
            $"TableName({Name})";
    }
}
