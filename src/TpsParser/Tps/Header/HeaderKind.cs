namespace TpsParser.Tps.Header
{
    /// <summary>
    /// Represents the header type and, if the header describes a table, the kind of information the table holds.
    /// </summary>
    public enum HeaderKind
    {
        /// <summary>
        /// Data
        /// </summary>
        Data = 0xF3,

        /// <summary>
        /// Metadata
        /// </summary>
        Metadata = 0xF6,

        /// <summary>
        /// Table definition
        /// </summary>
        TableDefinition = 0xFA,

        /// <summary>
        /// Memo table
        /// </summary>
        Memo = 0xFC,

        /// <summary>
        /// Table name descriptor
        /// </summary>
        TableName = 0xFE
    }
}
