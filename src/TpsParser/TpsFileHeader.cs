using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace TpsParser;

/// <summary>
/// Represents a TopSpeed file header.
/// </summary>
public sealed record TpsFileHeader
{
    /// <summary>
    /// The magic number used to identify a TopSpeed file.
    /// </summary>
    public const string TopSpeedMagicNumber = "tOpS";

    /// <summary>
    /// Gets the base address of the file. For well-formed TopSpeed files, this is always zero.
    /// </summary>
    public int Address { get; init; }

    /// <summary>
    /// Gets the number of bytes in the file header.
    /// </summary>
    public int HeaderSize { get; init; }

    /// <summary>
    /// Gets the length of the file.
    /// </summary>
    /// <remarks>
    /// Reverse-engineering note: This appears to always be the same as <see cref="FileLength2"/>.
    /// It may be used by the TopSpeed database driver to detect incomplete writes.
    /// </remarks>
    public int FileLength1 { get; init; }

    /// <summary>
    /// Gets the length of the file.
    /// </summary>
    /// <remarks>
    /// Reverse-engineering note: This appears to always be the same as <see cref="FileLength1"/>.
    /// It may be used by the TopSpeed database driver to detect incomplete writes.
    /// </remarks>
    public int FileLength2 { get; init; }

    /// <summary>
    /// Gets the magic number signature in the TopSpeed file header. This should be 'tOpS' for all TPS files.
    /// </summary>
    public required string MagicNumber { get; init; }

    /// <summary></summary>
    /// <remarks>
    /// Reverse-engineering note: This appears to always be zero.
    /// </remarks>
    public short Zeroes { get; init; }

    /// <summary>
    /// Gets the last issued row number in the file.
    /// </summary>
    public int LastIssuedRow { get; init; }

    /// <summary>
    /// Gets the number of changes made to the file by the TopSpeed database driver.
    /// </summary>
    public int Changes { get; init; }

    /// <summary>
    /// Gets the offset to the management block.
    /// </summary>
    public uint ManagementBlockOffset { get; init; }

    /// <summary>
    /// <para>
    /// Gets the array of locations and sizes of <see cref="TpsBlock"/> objects that are stored in the file.
    /// </para>
    /// <para>
    /// The header has predefined space for 60 page descriptors.
    /// For smaller files, most of the pages will have a length of zero.
    /// </para>
    /// </summary>
    public ImmutableArray<TpsBlockDescriptor> BlockDescriptors { get; init; }

    /// <summary>
    /// Returns true if the header represents a valid TopSpeed file.
    /// </summary>
    public bool IsTopSpeedFile => MagicNumber == TopSpeedMagicNumber;

    /// <summary>
    /// Creates a new <see cref="TpsFileHeader"/> by parsing the data from the given <see cref="TpsRandomAccess"/> reader.
    /// </summary>
    /// <param name="rx"></param>
    /// <returns></returns>
    /// <exception cref="TpsParserException"></exception>
    public static TpsFileHeader Parse(TpsRandomAccess rx)
    {
        ArgumentNullException.ThrowIfNull(rx);

        int address = rx.ReadLongLE();

        if (address != 0)
        {
            throw new TpsParserException("File does not start with 0x00000000. It is not a TopSpeed file or it may be encrypted.");
        }

        short headerSize = rx.ReadShortLE();

        var header = rx.Read(headerSize - 6);

        int fileLength1 = header.ReadLongLE();
        int fileLength2 = header.ReadLongLE();
        string magicNumber = header.ReadFixedLengthString(4, Encoding.ASCII); // Always ASCII
        short zeroes = header.ReadShortLE();
        int lastIssuedRow = header.ReadLongBE();
        int changes = header.ReadLongLE();
        uint managementBlockOffset = GetFileOffset(header.ReadLongLE());

        // 60 blocks are hard-defined in the header but many of them will be zero-length and/or duplicates.
        const int NumberOfBlocks = 60;

        var pageRanges = new TpsBlockDescriptor[NumberOfBlocks];

        var pageStartRx = header.Read(length: NumberOfBlocks * 4 /* Four bytes per integer */);
        var pageEndRx = header;

        for (int i = 0; i < NumberOfBlocks; i++)
        {
            int startPageRef = pageStartRx.ReadLongLE();
            int endPageRef = pageEndRx.ReadLongLE();

            uint startOffset = GetFileOffset(startPageRef);
            uint endOffset = GetFileOffset(endPageRef);

            if (endOffset < startOffset)
            {
                throw new TpsParserException($"Malformed block descriptor at index ({i}): EndOffset (0x{endOffset:8x}) is before StartOffset (0x{startOffset:8x}).");
            }

            pageRanges[i] = new TpsBlockDescriptor(
                StartOffset: startOffset,
                EndOffset: endOffset);
        }

        return new TpsFileHeader
        {
            Address = address,
            HeaderSize = headerSize,
            FileLength1 = fileLength1,
            FileLength2 = fileLength2,
            MagicNumber = magicNumber,
            Zeroes = zeroes,
            LastIssuedRow = lastIssuedRow,
            Changes = changes,
            ManagementBlockOffset = managementBlockOffset,
            BlockDescriptors = [.. pageRanges]
        };
    }

    private static uint GetFileOffset(int blockReference) => (uint)((blockReference << 8) + 0x200);

    /// <inheritdoc/>
    public bool Equals(TpsFileHeader? other)
    {
        return other is not null
            && Address == other.Address
            && HeaderSize == other.HeaderSize
            && FileLength1 == other.FileLength1
            && FileLength2 == other.FileLength2
            && MagicNumber == other.MagicNumber
            && Zeroes == other.Zeroes
            && LastIssuedRow == other.LastIssuedRow
            && Changes == other.Changes
            && ManagementBlockOffset == other.ManagementBlockOffset
            && BlockDescriptors.SequenceEqual(other.BlockDescriptors);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            Address,
            HeaderSize,
            FileLength1,
            FileLength2,
            MagicNumber,
            Zeroes,
            LastIssuedRow,
            Changes);
    }
}
