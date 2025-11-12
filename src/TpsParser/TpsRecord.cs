using System;

namespace TpsParser;

/// <summary>
/// Represents a record within a TPS file.
/// </summary>
public sealed record TpsRecord
{
    /// <summary></summary>
    public byte Flags { get; init; }

    /// <summary>
    /// Returns <see langword="true"/> if <see cref="Flags"/> indicates that the record data has <see cref="PayloadTotalLength"/>;
    /// <see langword="false"/> if it was inherited from the previous record.
    /// </summary>
    public bool OwnsPayloadTotalLength => (Flags & 0x80) != 0;

    /// <summary>
    /// Returns <see langword="true"/> if <see cref="Flags"/> indicates that the record data has <see cref="PayloadHeaderLength"/>;
    /// <see langword="false"/> if it was inherited from the previous record.
    /// </summary>
    public bool OwnsPayloadHeaderLength => (Flags & 0x40) != 0;

    /// <summary>
    /// From <see cref="Flags"/>, gets the number of bytes (no more than 63) that are copied from the previous <see cref="TpsRecord"/> payload.
    /// </summary>
    public byte PayloadInheritedBytes => (byte)(Flags & 0x3F);

    /// <summary>
    /// Gets the length of the payload in bytes, including payload header.
    /// </summary>
    public ushort PayloadTotalLength { get; init; }

    /// <summary>
    /// Gets the length of the payload header in bytes.
    /// </summary>
    public ushort PayloadHeaderLength { get; init; }

    /// <summary>
    /// <para>
    /// Gets a memory region that reflects the data for this <see cref="TpsRecord"/> before parsing.
    /// The data includes the header, payload header, and payload content.
    /// </para>
    /// <para>
    /// If the record has partial data (either <see cref="OwnsPayloadTotalLength"/> or <see cref="OwnsPayloadHeaderLength"/> are <see langword="false"/>)
    /// then the payload header and content needs to be copied from the previous record in the <see cref="TpsPage"/>.
    /// For partial records, this memory region reflects the record data before copying.
    /// </para>
    /// </summary>
    public required ReadOnlyMemory<byte> RecordData { get; init; }

    /// <summary>
    /// Gets a memory region that reflects all of the payload header and content data.
    /// </summary>
    public required ReadOnlyMemory<byte> PayloadData { get; init; }

    /// <summary>
    /// Creates a new <see cref="TpsRecord"/>. This is typically done on the first of a list.
    /// </summary>
    /// <param name="rx"></param>
    public static TpsRecord Parse(TpsRandomAccess rx)
    {
        ArgumentNullException.ThrowIfNull(rx);

        var incomingRecordData = rx.PeekRemainingMemory();

        byte flags = rx.ReadByte();

        if ((flags & 0xC0) != 0xC0)
        {
            throw new TpsParserException($"Cannot construct a TpsRecord without record and header lengths (Flags = 0x{flags:x2}).");
        }

        ushort payloadTotalLength = rx.ReadUnsignedShortLE();
        ushort payloadHeaderLength = rx.ReadUnsignedShortLE();

        if (payloadHeaderLength > payloadTotalLength)
        {
            throw new TpsParserException($"Payload header length ({payloadHeaderLength}) exceeds the total payload length ({payloadTotalLength}).");
        }

        // Memory region from the flags to the end of the payload.
        // Ensure we're not including extra data at the end.
        var actualRecordData = incomingRecordData[..(
            sizeof(byte)
            + sizeof(ushort)
            + sizeof(ushort)
            + payloadTotalLength)];

        var payloadRx = rx.Read(payloadTotalLength);

        var payloadData = payloadRx.PeekRemainingMemory();

        return new TpsRecord
        {
            Flags = flags,
            PayloadTotalLength = payloadTotalLength,
            PayloadHeaderLength = payloadHeaderLength,
            RecordData = actualRecordData,
            PayloadData = payloadData
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

        var incomingRecordData = rx.PeekRemainingMemory();

        byte flags = rx.ReadByte();

        ushort payloadTotalLength;

        bool hasPayloadTotalLength = (flags & 0x80) != 0;
        bool hasPayloadHeaderLength = (flags & 0x40) != 0;

        if (hasPayloadTotalLength)
        {
            payloadTotalLength = rx.ReadUnsignedShortLE();
        }
        else
        {
            payloadTotalLength = previous.PayloadTotalLength;
        }

        ushort payloadHeaderLength;

        if (hasPayloadHeaderLength)
        {
            payloadHeaderLength = rx.ReadUnsignedShortLE();
        }
        else
        {
            payloadHeaderLength = previous.PayloadHeaderLength;
        }

        if (payloadHeaderLength > payloadTotalLength)
        {
            throw new TpsParserException($"Payload header length ({payloadHeaderLength}) exceeds the total payload length ({payloadTotalLength}).");
        }

        int bytesToCopy = flags & 0x3F; // no more than 63 bytes

        // Memory region from the flags to the end of the payload before copying.
        // Ensure we're not including extra data at the end.
        var actualRecordData = incomingRecordData[..(
            sizeof(byte)
            + (hasPayloadTotalLength ? sizeof(ushort) : 0)
            + (hasPayloadHeaderLength ? sizeof(ushort) : 0)
            + payloadTotalLength
            - bytesToCopy)];

        byte[] newData = new byte[payloadTotalLength];
        var newDataMemory = newData.AsMemory();

        if (bytesToCopy > payloadTotalLength)
        {
            throw new TpsParserException($"Number of bytes to copy ({bytesToCopy}) exceeds the record length ({payloadTotalLength}).");
        }

        previous.PayloadData[..bytesToCopy].CopyTo(newData);
        
        rx.ReadBytes(payloadTotalLength - bytesToCopy).CopyTo(newDataMemory[bytesToCopy..]);

        var newRx = new TpsRandomAccess(newData, rx.Encoding);

        if (newRx.Length != payloadTotalLength)
        {
            throw new TpsParserException($"Data and record length mismatch: expected {payloadTotalLength} but was {newRx.Length}.");
        }
        
        var payloadRx = newRx.Read(payloadTotalLength);

        var payloadData = payloadRx.PeekRemainingMemory();

        return new TpsRecord
        {
            Flags = flags,
            PayloadTotalLength = payloadTotalLength,
            PayloadHeaderLength = payloadHeaderLength,
            RecordData = actualRecordData,
            PayloadData = payloadData,
        };
    }

    /// <summary>
    /// Calculates the payload type code from the payload header, if available.
    /// </summary>
    public RecordPayloadType? PayloadType
    {
        get
        {
            if (PayloadData.Length < 5)
            {
                return null;
            }

            if (PayloadData.Span[0] == (byte)RecordPayloadType.TableName)
            {
                return RecordPayloadType.TableName;
            }

            return (RecordPayloadType)PayloadData.Span[4];
        }
    }

    /// <summary>
    /// Parses the payload data into a strongly-typed object.
    /// </summary>
    /// <returns></returns>
    public IRecordPayload? GetPayload()
    {
        var payloadType = PayloadType;

        if (payloadType is null)
        {
            return null;
        }

        return payloadType switch
        {
            RecordPayloadType.TableName => new TableNameRecordPayload { PayloadData = PayloadData, PayloadHeaderLength = PayloadHeaderLength },
            RecordPayloadType.Data => new DataRecordPayload { PayloadData = PayloadData },
            RecordPayloadType.Metadata => new MetadataRecordPayload { PayloadData = PayloadData },
            RecordPayloadType.TableDef => new TableDefinitionRecordPayload { PayloadData = PayloadData },
            RecordPayloadType.Memo => new MemoRecordPayload { PayloadData = PayloadData },
            RecordPayloadType.Index => new IndexRecordPayload { PayloadData = PayloadData },
            _ => new IndexRecordPayload { PayloadData = PayloadData }
        };
    }
}
