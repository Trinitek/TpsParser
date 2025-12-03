namespace TpsParser;

/// <summary>
/// Represents the type of record and what information it describes.
/// </summary>
public enum RecordPayloadType : byte
{
    /// <summary>
    /// A section that contains a database record.
    /// </summary>
    Data        = 0xF3,

    /// <summary>
    /// A section that contains metadata about other records.
    /// </summary>
    Metadata    = 0xF6,

    /// <summary>
    /// A section that contains a <see cref="TableDefinition"/>.
    /// Sufficiently large table definitions will have two or more <see cref="TableDef"/> records.
    /// </summary>
    TableDef    = 0xFA,

    /// <summary>
    /// A section that contains an index.
    /// </summary>
    Index       = 0xFB,

    /// <summary>
    /// A section that contains a <c>MEMO</c> or <c>BLOB</c>.
    /// Sufficiently large objects will have two or more <see cref="Memo"/> records.
    /// </summary>
    Memo        = 0xFC,

    /// <summary>
    /// A section that contains the name of a table.
    /// </summary>
    TableName   = 0xFE,
}
