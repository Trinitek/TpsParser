using System;

namespace TpsParser.Tps;

/// <summary>
/// Represents a record within a TPS file.
/// </summary>
public sealed record TpsRecord
{
    /// <summary></summary>
    public byte Flags { get; init; }

    /// <summary>
    /// True if <see cref="Flags"/> indicates that the record data has <see cref="RecordLength"/>; false if it was inherited from the previous record.
    /// </summary>
    public bool OwnsRecordLength => (Flags & 0x80) != 0;

    /// <summary>
    /// True if <see cref="Flags"/> indicates that the record data has <see cref="HeaderLength"/>; false if it was inherited from the previous record.
    /// </summary>
    public bool OwnsHeaderLength => (Flags & 0x40) != 0;

    /// <summary>
    /// From <see cref="Flags"/>, gets the number of bytes (no more than 63) that describe the next <see cref="TpsRecord"/>.
    /// </summary>
    public byte NextRecordBytes => (byte)(Flags & 0x3F);

    /// <summary>
    /// Gets the length of the record in bytes.
    /// </summary>
    public ushort RecordLength { get; init; }

    /// <summary>
    /// Gets the length of the header in bytes.
    /// </summary>
    public ushort HeaderLength { get; init; }

    /// <summary>
    /// Gets the header defined in this record, if any.
    /// </summary>
    public IHeader? Header { get; init; }

    /// <summary>
    /// Gets the <see cref="TpsRandomAccess"/> reader used to access the data for this record.
    /// </summary>
    public required TpsRandomAccess DataRx { internal get; init; }

    /// <summary>
    /// Creates a new <see cref="TpsRecord"/>. This is typically done on the first of a list.
    /// </summary>
    /// <param name="rx">The data to read from.</param>
    public static TpsRecord Parse(TpsRandomAccess rx)
    {
        ArgumentNullException.ThrowIfNull(rx);

        byte flags = rx.ReadByte();

        if ((flags & 0xC0) != 0xC0)
        {
            throw new TpsParserException($"Cannot construct a TpsRecord without record and header lengths (0x{flags:x2})");
        }

        ushort recordLength = rx.ReadUnsignedShortLE();
        ushort headerLength = rx.ReadUnsignedShortLE();

        var newRx = rx.Read(recordLength);

        var headerRx = newRx.Read(headerLength);

        var header = BuildHeader(headerRx);

        return new TpsRecord
        {
            Flags = flags,
            RecordLength = recordLength,
            HeaderLength = headerLength,
            Header = header,
            DataRx = newRx
        };
    }

    /// <summary>
    /// Creates a new <see cref="TpsRecord"/> by partially copying the previous one.
    /// </summary>
    /// <param name="previous">The previous record.</param>
    /// <param name="rx">The data to read from.</param>
    public static TpsRecord Parse(TpsRecord previous, TpsRandomAccess rx)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(rx);

        byte flags = rx.ReadByte();

        ushort recordLength;

        if ((flags & 0x80) != 0)
        {
            recordLength = rx.ReadUnsignedShortLE();
        }
        else
        {
            recordLength = previous.RecordLength;
        }

        ushort headerLength;

        if ((flags & 0x40) != 0)
        {
            headerLength = rx.ReadUnsignedShortLE();
        }
        else
        {
            headerLength = previous.HeaderLength;
        }

        int bytesToCopy = flags & 0x3F; // no more than 63 bytes

        byte[] newData = new byte[recordLength];
        var newDataMemory = newData.AsMemory();

        if (bytesToCopy > recordLength)
        {
            throw new TpsParserException($"Number of bytes to copy ({bytesToCopy}) exceeds the record length ({recordLength}).");
        }

        previous.DataRx.PeekBaseSpan()[..bytesToCopy].CopyTo(newData);
        
        // TODO test coverage for well-formed memory copy
        rx.ReadBytes(recordLength - bytesToCopy).CopyTo(newDataMemory[bytesToCopy..]);

        var newRx = new TpsRandomAccess(newData, rx.Encoding);

        if (newRx.Length != recordLength)
        {
            throw new TpsParserException($"Data and record length mismatch: expected {recordLength} but was {newRx.Length}.");
        }

        var headerRx = newRx.Read(headerLength);

        var header = BuildHeader(headerRx);

        return new TpsRecord
        {
            Flags = flags,
            RecordLength = recordLength,
            HeaderLength = headerLength,
            Header = header,
            DataRx = newRx
        };
    }

    private static IHeader? BuildHeader(TpsRandomAccess rx)
    {
        if (rx.Length < 5)
        {
            return null;
        }

        if (rx.PeekByte(0) == (byte)RecordType.TableName)
        {
            var preHeader = PreHeader.Parse(rx, readTableNumber: false);

            IHeader header = TableNameHeader.Parse(preHeader, rx);

            return header;
        }
        else
        {
            var preHeader = PreHeader.Parse(rx, readTableNumber: true);

            IHeader header = preHeader.Type switch
            {
                RecordType.TableName => TableNameHeader.Parse(preHeader, rx),
                RecordType.Data => DataHeader.Parse(preHeader, rx),
                RecordType.Metadata => MetadataHeader.Parse(preHeader, rx),
                RecordType.TableDef => TableDefinitionHeader.Parse(preHeader, rx),
                RecordType.Memo => MemoHeader.Parse(preHeader, rx),
                RecordType.Index => IndexHeader.Parse(preHeader, rx),
                _ => IndexHeader.Parse(preHeader, rx)
            };

            return header;
        }
    }
}
