using System;
using System.Linq;

namespace TpsParser;

/// <summary>
/// Represents the schema for a <c>MEMO</c> or <c>BLOB</c>.
/// </summary>
public sealed record MemoDefinition
{
    /// <summary>
    /// If the <c>MEMO</c> or <c>BLOB</c> is stored in an external file, gets the name of that file.
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
    /// Gets the maximum number of bytes in the <c>MEMO</c> or <c>BLOB</c> content.
    /// </summary>
    public ushort Length { get; init; }

    /// <summary></summary>
    public ushort Flags { get; init; }

    /// <summary>
    /// Returns <see langword="true"/> if the record has the <c>BINARY</c> attribute.
    /// </summary>
    /// <remarks>
    /// Reverse-engineering note: the presence or lack of this attribute does not seem to affect how
    /// data is read. The only thing that matters is whether the data is <c>MEMO</c> or <c>BLOB</c>.
    /// </remarks>
    public bool HasBinaryAttribute => (Flags & 0x02) != 0;

    /// <summary>
    /// Returns <see langword="true"/> if the record contains <c>MEMO</c> data; <see langword="false"/> if it contains <c>BLOB</c> data.
    /// </summary>
    public bool IsTextMemo => (Flags & 0x04) == 0;

    /// <summary>
    /// Returns <see langword="true"/> if the record contains <c>BLOB</c> data; <see langword="false"/> if it contains <c>MEMO</c> data.
    /// </summary>
    public bool IsBlob => !IsTextMemo;

    /// <summary>
    /// Creates a new <see cref="MemoDefinition"/> using the given data reader.
    /// </summary>
    /// <param name="rx"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static MemoDefinition Parse(TpsRandomAccess rx)
    {
        ArgumentNullException.ThrowIfNull(rx);

        string externalFileName = rx.ReadZeroTerminatedString();

        if (externalFileName.Length == 0)
        {
            byte memoMarker = rx.ReadByte();

            if (memoMarker != 1)
            {
                throw new TpsParserException($"Bad memo definition: missing 0x01 after zero string. Was 0x{memoMarker:x2}.");
            }
        }

        string fullName = rx.ReadZeroTerminatedString();
        ushort length = rx.ReadUnsignedShortLE();
        ushort flags = rx.ReadUnsignedShortLE();

        return new MemoDefinition
        {
            ExternalFileName = externalFileName,
            FullName = fullName,
            Length = length,
            Flags = flags
        };
    }
}
