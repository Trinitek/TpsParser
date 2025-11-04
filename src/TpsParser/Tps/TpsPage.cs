using System;
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
    public int Address { get; init; }

    /// <summary>
    /// Gets the size of the page in bytes.
    /// </summary>
    public ushort Size { get; init; }

    /// <summary>
    /// Gets the size of the page in bytes when uncompressed.
    /// </summary>
    public ushort SizeUncompressed { get; init; }

    /// <summary>
    /// Gets the size of the page without the header in bytes when uncompressed.
    /// </summary>
    public ushort SizeUncompressedWithoutHeader { get; init; }

    /// <summary>
    /// Gets the number of records in this page.
    /// </summary>
    public ushort RecordCount { get; init; }

    public byte Flags { get; init; }

    /// <summary>
    /// Gets the <see cref="TpsRandomAccess"/> reader used to access the (probably) compressed data for this page.
    /// </summary>
    public required TpsRandomAccess CompressedDataRx { private get; init; }

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

        int address = rx.ReadLongLE();
        ushort size = rx.ReadUnsignedShortLE();

        var header = rx.Read(size - 6);

        ushort sizeUncompressed = header.ReadUnsignedShortLE();
        ushort sizeUncompressedWithoutHeader = header.ReadUnsignedShortLE();
        ushort recordCount = header.ReadUnsignedShortLE();
        byte flags = header.ReadByte();

        var compressedDataRx = header.Read(size - 13);

        return new TpsPage
        {
            Address = address,
            Size = size,
            SizeUncompressed = sizeUncompressed,
            SizeUncompressedWithoutHeader = sizeUncompressedWithoutHeader,
            RecordCount = recordCount,
            Flags = flags,
            CompressedDataRx = compressedDataRx
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
