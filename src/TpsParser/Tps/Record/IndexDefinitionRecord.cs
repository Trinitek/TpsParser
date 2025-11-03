using System;
using System.Collections.Immutable;
using TpsParser.Binary;

namespace TpsParser.Tps.Record;

/// <summary></summary>
public enum SortDirection
{
    /// <summary></summary>
    Ascending   = 0,

    /// <summary></summary>
    Descending  = 1
}

/// <summary>
/// Associates various index or key properties with a field.
/// </summary>
/// <param name="FieldIndex"></param>
/// <param name="Flags"></param>
public sealed record KeyField(ushort FieldIndex, ushort Flags)
{
    /// <summary></summary>
    public SortDirection SortDirection =>
        (Flags & 0x1) == 0
        ? SortDirection.Ascending
        : SortDirection.Descending;
}

/// <summary></summary>
[Flags]
public enum IndexDefinitionFlags : byte
{
    /// <summary>
    /// Clarion keyword <c>DUP</c>. Allows multiple records with duplicate values.
    /// </summary>
    AllowDuplicates = 0b0000_0001,

    /// <summary>
    /// Clarion keyword <c>OPT</c>. Records with null values (zero or blank) are excluded from the index.
    /// </summary>
    AllowNull       = 0b0000_0010,

    /// <summary>
    /// Clarion keyword <c>NOCASE</c>. Sort order is case-insensitive.
    /// </summary>
    CaseInsensitive = 0b0000_0100,

    /// <summary>
    /// Clarion keyword <c>PRIMARY</c>. This key is the table's relational primary key.
    /// </summary>
    PrimaryKey      = 0b0001_0000,
}

/// <summary>
/// Represents the schema for an index.
/// </summary>
public sealed record IndexDefinitionRecord
{
    /// <summary>
    /// If the key or index is stored in an external file, gets the name of that file.
    /// </summary>
    public required string ExternalFile { get; init; }

    /// <summary>
    /// Gets an array of fields managed by this key or index.
    /// </summary>
    public required ImmutableArray<KeyField> KeyFields { get; init; }

    /// <summary>
    /// Gets the flags for this key or index.
    /// </summary>
    public IndexDefinitionFlags Flags { get; init; }

    /// <summary>
    /// Gets the name of the index.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the number of fields tracked by the key or index.
    /// </summary>
    public ushort FieldCount { get; init; }

    /// <summary>
    /// Creates a new <see cref="IndexDefinitionRecord"/> by parsing the data from the given <see cref="TpsRandomAccess"/> reader.
    /// </summary>
    /// <param name="rx"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static IndexDefinitionRecord Parse(TpsRandomAccess rx)
    {
        ArgumentNullException.ThrowIfNull(rx);

        string externalFile = rx.ReadZeroTerminatedString();

        if (externalFile.Length == 0)
        {
            int read = rx.ReadByte();

            if (read != 0x01)
            {
                throw new ArgumentException($"Bad index definition: missing 0x01 after zero string ({read:X2})");
            }
        }

        string name = rx.ReadZeroTerminatedString();
        IndexDefinitionFlags flags = (IndexDefinitionFlags)rx.ReadByte();
        ushort fieldsInKey = rx.ReadUnsignedShortLE();

        KeyField[] keys = new KeyField[fieldsInKey];

        for (int i = 0; i < fieldsInKey; i++)
        {
            keys[i] = new(
                FieldIndex: rx.ReadUnsignedShortLE(),
                Flags: rx.ReadUnsignedShortLE());
        }

        return new IndexDefinitionRecord
        {
            ExternalFile = externalFile,
            Name = name,
            Flags = flags,
            FieldCount = fieldsInKey,
            KeyFields = [.. keys]
        };
    }
}
