using System;
using TpsParser.Binary;

namespace TpsParser.Tps.Record;

/// <summary>
/// Represents the schema for an index.
/// </summary>
public sealed record IndexDefinitionRecord
{
    public required string ExternalFile { get; init; }
    public required short[] KeyField { get; init; }
    public required short[] KeyFieldFlag { get; init; }
    private byte Flags { get; init; }

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

        var keyField = new short[fieldsInKey];
        var keyFieldFlag = new short[fieldsInKey];

        for (int i = 0; i < fieldsInKey; i++)
        {
            keyField[i] = rx.ReadShortLE();
            keyFieldFlag[i] = rx.ReadShortLE();
        }

        return new IndexDefinitionRecord
        {
            ExternalFile = externalFile,
            Name = name,
            Flags = flags,
            FieldsInKey = fieldsInKey,
            KeyField = keyField,
            KeyFieldFlag = keyFieldFlag
        };
    }
}
