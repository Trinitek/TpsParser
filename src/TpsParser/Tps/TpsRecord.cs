using System;
using System.Buffers.Binary;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using TpsParser.Tps.Record;

namespace TpsParser.Tps;

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
    /// Gets the payload defined in this record, if any.
    /// </summary>
    public required IRecordPayload? Payload { get; init; }

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

        var payload = BuildPayload(payloadRx, payloadHeaderLength: payloadHeaderLength);

        return new TpsRecord
        {
            Flags = flags,
            PayloadTotalLength = payloadTotalLength,
            PayloadHeaderLength = payloadHeaderLength,
            RecordData = actualRecordData,
            Payload = payload,
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

        var payload = BuildPayload(payloadRx, payloadHeaderLength: payloadHeaderLength);

        return new TpsRecord
        {
            Flags = flags,
            PayloadTotalLength = payloadTotalLength,
            PayloadHeaderLength = payloadHeaderLength,
            RecordData = actualRecordData,
            Payload = payload,
            PayloadData = payloadData,
        };
    }

    private static IRecordPayload? BuildPayload(TpsRandomAccess rx, ushort payloadHeaderLength)
    {
        if (rx.Length < 5)
        {
            return null;
        }

        if (rx.PeekByte(0) == (byte)RecordPayloadType.TableName)
        {
            return TableNameRecordPayload.Parse(rx, payloadHeaderLength);
        }

        var payloadType = (RecordPayloadType)rx.PeekByte(4);

        return payloadType switch
        {
            RecordPayloadType.Data => DataRecordPayload.Parse(rx),
            RecordPayloadType.Metadata => MetadataRecordPayload.Parse(rx),
            RecordPayloadType.TableDef => TableDefinitionRecordPayload.Parse(rx),
            RecordPayloadType.Memo => MemoRecordPayload.Parse(rx),
            RecordPayloadType.Index => IndexRecordPayload.Parse(rx),
            _ => IndexRecordPayload.Parse(rx)
        };
    }
}

public interface IRecordPayload;

public interface IPayloadTableNumber
{
    /// <summary>
    /// Gets the table number to which this record belongs.
    /// </summary>
    int TableNumber { get; }
}

public interface IPayloadRecordNumber
{
    /// <summary>
    /// Gets the record number to which this record belongs.
    /// </summary>
    int RecordNumber { get; }
}

public sealed record IndexRecordPayload : IRecordPayload, IPayloadTableNumber, IPayloadRecordNumber
{
    /// <inheritdoc cref="IPayloadTableNumber.TableNumber"/>
    public int TableNumber { get; init; }

    /// <summary>
    /// Gets the index number of the corresponding definition in <see cref="TableDefinition.Indexes"/>.
    /// </summary>
    public byte DefinitionIndex { get; init; }

    /// <summary>
    /// Gets the number of the <see cref="TpsRecord"/> with payload type <see cref="DataRecordPayload"/> to which this index belongs.
    /// </summary>
    public int RecordNumber { get; init; }

    /// <summary>
    /// Creates a new <see cref="IndexRecordPayload"/> from the given data reader.
    /// </summary>
    /// <param name="rx"></param>
    /// <returns></returns>
    public static IndexRecordPayload Parse(TpsRandomAccess rx)
    {
        var span = rx.PeekRemainingSpan();

        int tableNumber = BinaryPrimitives.ReadInt32BigEndian(span[0..]);
        byte indexNumber = span[4];

        var recordNumber = BinaryPrimitives.ReadInt32BigEndian(span[^4..]);

        return new IndexRecordPayload
        {
            TableNumber = tableNumber,
            DefinitionIndex = indexNumber,
            RecordNumber = recordNumber
        };
    }
}

public sealed record MemoRecordPayload : IRecordPayload, IPayloadTableNumber, IPayloadRecordNumber
{
    /// <inheritdoc cref="IPayloadTableNumber.TableNumber"/>
    public int TableNumber { get; init; }

    /// <summary>
    /// Gets the number of the <see cref="IDataRecord"/> that owns this <c>MEMO</c>.
    /// </summary>
    public int RecordNumber { get; init; }

    /// <summary>
    /// Gets the sequence number of the <c>MEMO</c> when the <c>MEMO</c> is segmented into multiple records. The first segment is 0.
    /// </summary>
    public ushort SequenceNumber { get; init; }

    /// <summary>
    /// Gets the index number of the corresponding definition in <see cref="TableDefinition.Memos"/>.
    /// </summary>
    public byte DefinitionIndex { get; init; }

    /// <summary>
    /// Gets the memory region of the content in this entry.
    /// </summary>
    /// <remarks>
    /// Reverse-engineering note: for text <c>MEMO</c>s, this seems to be at most 256 bytes.
    /// </remarks>
    public required ReadOnlyMemory<byte> Content { get; init; }

    /// <summary>
    /// Creates a new <see cref="MemoRecordPayload"/> from the given data reader.
    /// </summary>
    /// <param name="rx"></param>
    /// <returns></returns>
    public static MemoRecordPayload Parse(TpsRandomAccess rx)
    {
        var mem = rx.PeekRemainingMemory();
        var span = mem.Span;

        int tableNumber = BinaryPrimitives.ReadInt32BigEndian(span[0..]);
        // byte payloadType = span[4];

        int recordNumber = BinaryPrimitives.ReadInt32BigEndian(span[5..]);
        byte memoIndex = span[9];
        ushort sequenceNumber = BinaryPrimitives.ReadUInt16BigEndian(span[10..]);
        var content = mem[12..];

        return new MemoRecordPayload
        {
            TableNumber = tableNumber,
            RecordNumber = recordNumber,
            DefinitionIndex = memoIndex,
            SequenceNumber = sequenceNumber,
            Content = content
        };
    }
}

public sealed record MetadataRecordPayload : IRecordPayload, IPayloadTableNumber
{
    /// <inheritdoc cref="IPayloadTableNumber.TableNumber"/>
    public int TableNumber { get; init; }

    /// <summary>
    /// Gets the type of record that this metadata describes.
    /// </summary>
    public RecordPayloadType AboutType { get; init; }

    /// <summary>
    /// Returns <see langword="true"/> if <see cref="Content"/> contains metadata about data records.
    /// </summary>
    public bool IsAboutData => AboutType == RecordPayloadType.Data;

    /// <summary>
    /// Returns <see langword="true"/> if <see cref="Content"/> contains metadata about an index.
    /// </summary>
    public bool IsAboutIndex => AboutType < RecordPayloadType.Data;

    /// <summary>
    /// Gets the memory region of the content in this entry.
    /// </summary>
    /// <remarks>
    /// Reverse-engineering notes: Data and Index metadata both include an extra 4 zero-bytes at the end of the content for all inspected files.
    /// Not sure what this is.
    /// </remarks>
    public required ReadOnlyMemory<byte> Content { get; init; }

    /// <summary>
    /// Attempts to parse the content as metadata about data records.
    /// </summary>
    /// <param name="metadata"></param>
    /// <returns></returns>
    public bool TryParseContentAsDataMetadata([NotNullWhen(true)] out DataMetadata? metadata)
    {
        if (!IsAboutData)
        {
            metadata = null;
            return false;
        }

        var span = Content.Span;

        int recordCount = BinaryPrimitives.ReadInt32LittleEndian(span);

        metadata = new DataMetadata(
            DataRecordCount: recordCount);

        return true;
    }

    /// <summary>
    /// Attempts to parse the content as metadata about an index.
    /// </summary>
    /// <param name="metadata"></param>
    /// <returns></returns>
    public bool TryParseContentAsIndexMetadata([NotNullWhen(true)] out IndexMetadata? metadata)
    {
        if (!IsAboutIndex)
        {
            metadata = null;
            return false;
        }

        var span = Content.Span;

        int recordCount = BinaryPrimitives.ReadInt32LittleEndian(span);

        metadata = new IndexMetadata(
            DataRecordCount: recordCount);

        return true;
    }

    /// <summary>
    /// Creates a new <see cref="MetadataRecordPayload"/> from the given data reader.
    /// </summary>
    /// <param name="rx"></param>
    /// <returns></returns>
    public static MetadataRecordPayload Parse(TpsRandomAccess rx)
    {
        var mem = rx.PeekRemainingMemory();
        var span = mem.Span;

        int tableNumber = BinaryPrimitives.ReadInt32BigEndian(span[0..]);
        // byte payloadType = span[4];

        RecordPayloadType aboutType = (RecordPayloadType)span[5];
        var content = mem[6..];

        return new MetadataRecordPayload
        {
            TableNumber = tableNumber,
            AboutType = aboutType,
            Content = content,
        };
    }
}

/// <summary>
/// Encapsulates metadata about data records in a table.
/// </summary>
/// <param name="DataRecordCount">The number of data records in the table.</param>
public sealed record DataMetadata(int DataRecordCount);

/// <summary>
/// Encapsulates metadata about an index.
/// </summary>
/// <param name="DataRecordCount">The number of data records in this index.</param>
public sealed record IndexMetadata(int DataRecordCount);

public sealed record TableDefinitionRecordPayload : IRecordPayload, IPayloadTableNumber
{
    /// <inheritdoc cref="IPayloadTableNumber.TableNumber"/>
    public int TableNumber { get; init; }

    /// <summary>
    /// Gets the sequence number of the <see cref="TableDefinition"/> when the definition is segmented into multiple records. The first segment is 0.
    /// </summary>
    public ushort SequenceNumber { get; init; }

    /// <summary>
    /// Gets the memory region of the content in this entry.
    /// </summary>
    /// <remarks>
    /// Reverse-engineering note: this seems to be at most 512 bytes.
    /// </remarks>
    public required ReadOnlyMemory<byte> Content { get; init; }

    /// <summary>
    /// Creates a new <see cref="TableDefinitionRecordPayload"/> from the given data reader.
    /// </summary>
    /// <param name="rx"></param>
    /// <returns></returns>
    public static TableDefinitionRecordPayload Parse(TpsRandomAccess rx)
    {
        var mem = rx.PeekRemainingMemory();
        var span = mem.Span;

        int tableNumber = BinaryPrimitives.ReadInt32BigEndian(span[0..]);
        // byte payloadType = span[4];

        ushort sequenceNumber = BinaryPrimitives.ReadUInt16LittleEndian(span[5..]);
        var content = mem[7..];

        return new TableDefinitionRecordPayload
        {
            TableNumber = tableNumber,
            SequenceNumber = sequenceNumber,
            Content = content,
        };
    }
}

public sealed record TableNameRecordPayload : IRecordPayload, IPayloadTableNumber
{
    /// <inheritdoc cref="IPayloadTableNumber.TableNumber"/>
    public int TableNumber { get; init; }

    /// <summary>
    /// Gets the name of the table.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Creates a new <see cref="TableNameRecordPayload"/> from the given data reader.
    /// </summary>
    /// <param name="rx"></param>
    /// <param name="payloadHeaderLength"></param>
    /// <returns></returns>
    public static TableNameRecordPayload Parse(TpsRandomAccess rx, ushort payloadHeaderLength)
    {
        var mem = rx.PeekRemainingMemory();
        var span = mem.Span;

        string name = rx.Encoding.GetString(span[1..payloadHeaderLength]);
        int tableNumber = BinaryPrimitives.ReadInt32BigEndian(span[payloadHeaderLength..]);

        return new TableNameRecordPayload
        {
            Name = name,
            TableNumber = tableNumber,
        };
    }
}

public sealed record DataRecordPayload : IRecordPayload, IPayloadTableNumber, IPayloadRecordNumber
{
    /// <inheritdoc cref="IPayloadTableNumber.TableNumber"/>
    public int TableNumber { get; init; }

    /// <inheritdoc cref="IPayloadRecordNumber.RecordNumber"/>
    public int RecordNumber { get; init; }

    /// <summary>
    /// Gets the memory region of the content in this entry.
    /// </summary>
    /// <remarks>
    /// Reverse-engineering note: these can be somewhat large compared to the other record types.
    /// You should expect the length of this to be equal to <see cref="TableDefinition.RecordLength"/>.
    /// </remarks>
    public required ReadOnlyMemory<byte> Content { get; init; }

    /// <summary>
    /// Creates a new <see cref="DataRecordPayload"/> from the given data reader.
    /// </summary>
    /// <param name="rx"></param>
    /// <returns></returns>
    public static DataRecordPayload Parse(TpsRandomAccess rx)
    {
        var mem = rx.PeekRemainingMemory();
        var span = mem.Span;

        int tableNumber = BinaryPrimitives.ReadInt32BigEndian(span[0..]);
        // byte payloadType = span[4];

        int recordNumber = BinaryPrimitives.ReadInt32BigEndian(span[5..]);
        var content = mem[9..];

        return new DataRecordPayload
        {
            TableNumber = tableNumber,
            RecordNumber = recordNumber,
            Content = content,
        };
    }
}
