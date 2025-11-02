using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using TpsParser.Binary;

namespace TpsParser.Tps;

/// <summary>
/// Represents a TopSpeed file header.
/// </summary>
public sealed record TpsFileHeader
{
    /// <summary>
    /// The magic number used to identify a TopSpeed file.
    /// </summary>
    public const string TopSpeedMagicNumber = "tOpS";

    public int Address { get; init; }
    public int HeaderSize { get; init; }
    public int FileLength1 { get; init; }
    public int FileLength2 { get; init; }

    /// <summary>
    /// Gets the magic number signature in the TopSpeed file header. This should be 'tOpS' for all TPS files.
    /// </summary>
    public required string MagicNumber { get; init; }

    public short Zeroes { get; init; }

    /// <summary>
    /// Gets the last issued row number in the file.
    /// </summary>
    public int LastIssuedRow { get; init; }

    public int Changes { get; init; }

    /// <summary>
    /// Gets the offset to the management page.
    /// </summary>
    public int ManagementPageReferenceOffset { get; init; }

    public ImmutableArray<TpsPageRange> PageRanges { get; init; }

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
        string magicNumber = header.ReadFixedLengthString(4);
        short zeroes = header.ReadShortLE();
        int lastIssuedRow = header.ReadLongBE();
        int changes = header.ReadLongLE();
        int managementPageReferenceOffset = TpsRandomAccess.GetFileOffset(header.ReadLongLE());

        // 60 pages are hard-defined in the header but most of them will be duplicates.
        const int maxNumberOfPages = 60;

        var pageRanges = new TpsPageRange[maxNumberOfPages];

        var pageStart = TpsRandomAccess.GetFileOffset(header.LongArrayLE(maxNumberOfPages));
        var pageEnd = TpsRandomAccess.GetFileOffset(header.LongArrayLE(maxNumberOfPages));

        for (int i = 0; i < maxNumberOfPages; i++)
        {
            pageRanges[i] = new TpsPageRange(
                StartOffset: pageStart[i],
                EndOffset: pageEnd[i]);
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
            ManagementPageReferenceOffset = managementPageReferenceOffset,
            PageRanges = [.. pageRanges]
        };
    }

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
            && ManagementPageReferenceOffset == other.ManagementPageReferenceOffset
            && PageRanges.SequenceEqual(other.PageRanges);
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

    ///// <inheritdoc/>
    //public override string ToString()
    //{
    //    var sb = new StringBuilder();
    //
    //    sb.AppendLine($"TpsHeader({StringUtils.ToHex8(Address)},{StringUtils.ToHex4(HeaderSize)},{StringUtils.ToHex8(FileLength1)},{StringUtils.ToHex8(FileLength2)}," +
    //        $"{MagicNumber},{StringUtils.ToHex4(Zeroes)},{StringUtils.ToHex8(LastIssuedRow)},{StringUtils.ToHex8(Changes)},{StringUtils.ToHex8(ManagementPageReferenceOffset)})");
    //
    //    for (int i = 0; i < PageStart.Length; i++)
    //    {
    //        sb.AppendLine($"{PageStart[i]}..{PageEnd[i]}");
    //    }
    //
    //    return sb.ToString();
    //}
}
