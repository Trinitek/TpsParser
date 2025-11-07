using System;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace TpsParser.Tps;

/// <summary>
/// Represents a page within a TPS file. Pages contain records.
/// </summary>
public sealed record TpsPage
{
    /// <summary>
    /// Gets the offset of the page within the file.
    /// </summary>
    public int AbsoluteAddress { get; init; }

    /// <summary>
    /// Gets the size of the page in bytes.
    /// </summary>
    public ushort Size { get; init; }

    /// <summary>
    /// Gets the size of the page in bytes when uncompressed.
    /// </summary>
    public ushort SizeUncompressed { get; init; }

    /// <summary>
    /// <para>
    /// Gets the size of the page in bytes when uncompressed and all <see cref="TpsRecord"/> data is fully expanded.
    /// </para>
    /// <para>
    /// When stored, records are deduplicated by partially referencing data from the previous record in the page.
    /// A record can save up to <see cref="TpsRecord.PayloadInheritedBytes"/>,
    /// plus 2 bytes when <see cref="TpsRecord.OwnsPayloadHeaderLength"/> is <see langword="false"/>,
    /// plus 2 bytes when <see cref="TpsRecord.OwnsPayloadTotalLength"/> is <see langword="false"/>.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Reverse-engineering note: guidance for the meaning of this field was gathered from a Russian-language
    /// document archived at <a href="https://web.archive.org/web/20140405102309/http://www.clarionlife.net/content/view/41/29/"/>.
    /// The document refers to this property as "Page length after decompression without any abbreviations."
    /// </remarks>
    public ushort SizeUncompressedExpanded { get; init; }

    /// <summary>
    /// Gets the number of records in this page.
    /// </summary>
    public ushort RecordCount { get; init; }

    /// <summary></summary>
    public byte Flags { get; init; }

    /// <summary>
    /// Gets the <see cref="TpsRandomAccess"/> reader used to access the (probably) compressed data for this page.
    /// </summary>
    public required TpsRandomAccess CompressedDataRx { private get; init; }

    /// <summary>
    /// Gets the memory region that reflects the data for this <see cref="TpsPage"/> before decompressing, including page header and metadata.
    /// </summary>
    public required ReadOnlyMemory<byte> PageData { get; init; }

    /// <summary>
    /// Gets the memory region for the (probably) compressed data for this page.
    /// </summary>
    public required ReadOnlyMemory<byte> CompressedData { get; init; }

    private TpsRandomAccess? _data;
    private IReadOnlyList<TpsRecord>? _records = null;

    /// <summary>
    /// Creates a new <see cref="TpsPage"/> by parsing the data from the given <see cref="TpsRandomAccess"/> reader.
    /// </summary>
    /// <param name="rx"></param>
    /// <returns></returns>
    public static TpsPage Parse(TpsRandomAccess rx)
    {
        ArgumentNullException.ThrowIfNull(rx);

        var mem = rx.PeekRemainingMemory();
        var span = mem.Span;

        int address = BinaryPrimitives.ReadInt32LittleEndian(span);
        ushort size = BinaryPrimitives.ReadUInt16LittleEndian(span[4..]);

        rx.JumpRelative(
            sizeof(int)
            + sizeof(ushort));

        var header = rx.Read(size - 6);

        ushort sizeUncompressed = BinaryPrimitives.ReadUInt16LittleEndian(span[6..]);
        ushort sizeUncompressedWithoutHeader = BinaryPrimitives.ReadUInt16LittleEndian(span[8..]);
        ushort recordCount = BinaryPrimitives.ReadUInt16LittleEndian(span[10..]);
        byte flags = span[12];

        header.JumpRelative(
            sizeof(ushort)
            + sizeof(ushort)
            + sizeof(ushort)
            + sizeof(byte));

        var compressedDataRx = header.Read(size - 13);

        // TODO need to update TpsBlock tests before we can constrain the mem length.
        var pageData = mem;
        //var pageData = mem[..(
        //    sizeof(int)
        //    + sizeof(ushort)
        //    + size)];

        var compressedData = mem[13..];

        return new TpsPage
        {
            AbsoluteAddress = address,
            Size = size,
            SizeUncompressed = sizeUncompressed,
            SizeUncompressedExpanded = sizeUncompressedWithoutHeader,
            RecordCount = recordCount,
            Flags = flags,
            CompressedDataRx = compressedDataRx,
            PageData = pageData,
            CompressedData = compressedData
        };
    }

    private TpsRandomAccess Decompress()
    {
        if ((Size != SizeUncompressed)
            && (Flags == 0))
        {
            try
            {
                CompressedDataRx.PushPosition();
                _data = CompressedDataRx.UnpackRunLengthEncoding();
            }
            catch (Exception ex)
            {
                throw new RunLengthEncodingException($"Bad RLE data block at index {CompressedDataRx} in {ToString()}", ex);
            }
            finally
            {
                CompressedDataRx.PopPosition();
            }
        }
        else
        {
            _data = CompressedDataRx;
        }

        return _data;
    }

    /// <summary>
    /// Gets all records in this page.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyList<TpsRecord> GetRecords()
    {
        if (_records is not null)
        {
            return _records;
        }

        var rx = Decompress();

        List<TpsRecord> records = new(RecordCount);
        
        // Skip pages with non 0x00 flags as they don't seem to contain TpsRecords.
        if (Flags == 0x00)
        {
            rx.PushPosition();

            try
            {
                TpsRecord? previousRecord = null;

                do
                {
                    TpsRecord currentRecord;

                    if (previousRecord is null)
                    {
                        currentRecord = TpsRecord.Parse(rx);
                    }
                    else
                    {
                        currentRecord = TpsRecord.Parse(previousRecord, rx);
                    }

                    records.Add(currentRecord);

                    previousRecord = currentRecord;
                }
                while (!rx.IsAtEnd && records.Count < RecordCount);
            }
            finally
            {
                rx.PopPosition();
            }
        }

        _records = records;

        return _records;
    }
}
