using System;
using System.Collections.Immutable;
using TpsParser.Binary;

namespace TpsParser.Tps.Record;


public enum SortDirection
{
    /// <summary></summary>
    Ascending   = 0,

    /// <summary></summary>
    Descending  = 1
}

public sealed record KeyField(ushort FieldIndex, ushort Flags)
{
    public SortDirection SortDirection =>
        (Flags & 0x1) == 0
        ? SortDirection.Ascending
        : SortDirection.Descending;
}

/// <summary>
/// Represents the schema for an index.
/// </summary>
public sealed record IndexDefinitionRecord
{
    public required string ExternalFile { get; init; }
    public required ImmutableArray<KeyField> Keys { get; init; }
    public byte Flags { get; init; }

    /// <summary>
    /// Gets the name of the index.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the number of fields tracked by the index.
    /// </summary>
    public ushort FieldsInKey { get; init; }

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
        byte flags = rx.ReadByte();
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
            FieldsInKey = fieldsInKey,
            Keys = [.. keys]
        };
    }
}
