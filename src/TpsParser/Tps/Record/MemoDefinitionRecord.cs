using System;
using System.Linq;
using TpsParser.Binary;

namespace TpsParser.Tps.Record;

/// <summary>
/// Represents the schema for a MEMO or BLOB.
/// </summary>
public sealed record MemoDefinitionRecord
{
    /// <summary>
    /// If the BLOB or MEMO is stored in an external file, gets the name of that file.
    /// </summary>
    public required string ExternalFileName { get; init; }

    /// <summary>
    /// <para>
    /// Gets the fully qualified name of the field with the table prefix, e.g. "INV:INVOICENO".
    /// Use <see cref="Name"/> for only the field name.
    /// </para>
    /// <para>
    /// If the table was not defined with a prefix in Clarion, then it will be absent.
    /// When present, it is rarely the same as the table name, if the table has a name at all.
    /// </para>
    /// </summary>
    public required string FullName { get; init; }

    /// <summary>
    /// <para>
    /// Gets the name of the field without the table prefix, e.g. "INVOICENO".
    /// Use <see cref="FullName"/> for the fully qualified field name.
    /// </para>
    /// </summary>
    public string Name => FullName.Split(':').Last();

    /// <summary>
    /// Gets the maximum number of bytes in the MEMO or BLOB.
    /// </summary>
    public ushort Length { get; init; }

    public short Flags { get; init; }

    /// <summary>
    /// Returns true if the record contains MEMO data; false if it contains BLOB data.
    /// </summary>
    public bool IsMemo => (Flags & 0x04) == 0;

    /// <summary>
    /// Returns true if the record contains BLOB data; false if it contains MEMO data.
    /// </summary>
    public bool IsBlob => !IsMemo;

    /// <summary>
    /// Creates a new <see cref="MemoDefinitionRecord"/> by parsing the data from the given <see cref="TpsRandomAccess"/> reader.
    /// </summary>
    /// <param name="rx"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static MemoDefinitionRecord Parse(TpsRandomAccess rx)
    {
        ArgumentNullException.ThrowIfNull(rx);

        string externalFileName = rx.ReadZeroTerminatedString();

        if (externalFileName.Length == 0)
        {
            byte memoMarker = rx.ReadByte();

            if (memoMarker != 1)
            {
                throw new ArgumentException($"Bad memo definition: missing 0x01 after zero string. Was 0x{memoMarker:x2}.");
            }
        }

        string fullName = rx.ReadZeroTerminatedString();
        ushort length = rx.ReadUnsignedShortLE();
        short flags = rx.ReadShortLE();

        return new MemoDefinitionRecord
        {
            ExternalFileName = externalFileName,
            FullName = fullName,
            Length = length,
            Flags = flags
        };
    }
}
