using System;
using System.Buffers.Binary;

namespace TpsParser;

public readonly record struct MemoRecordPayload : IRecordPayload, IPayloadTableNumber, IPayloadRecordNumber
{
    public required ReadOnlyMemory<byte> PayloadData { get; init; }

    /// <inheritdoc cref="IPayloadTableNumber.TableNumber"/>
    public readonly int TableNumber => BinaryPrimitives.ReadInt32BigEndian(PayloadData.Span[0..]);

    /// <summary>
    /// Gets the number of the associated data record that owns this <c>MEMO</c>.
    /// </summary>
    public readonly int RecordNumber => BinaryPrimitives.ReadInt32BigEndian(PayloadData.Span[5..]);

    /// <summary>
    /// Gets the sequence number of the <c>MEMO</c> when the <c>MEMO</c> is segmented into multiple records. The first segment is 0.
    /// </summary>
    public readonly ushort SequenceNumber => BinaryPrimitives.ReadUInt16BigEndian(PayloadData.Span[10..]);

    /// <summary>
    /// Gets the index number of the corresponding definition in <see cref="TableDefinition.Memos"/>.
    /// </summary>
    public readonly byte DefinitionIndex => PayloadData.Span[9];

    /// <summary>
    /// Gets the memory region of the content in this entry.
    /// </summary>
    /// <remarks>
    /// Reverse-engineering note: for text <c>MEMO</c>s, this seems to be at most 256 bytes.
    /// </remarks>
    public readonly ReadOnlyMemory<byte> Content => PayloadData[12..];

    public static MemoRecordPayload Create(
        int tableNumber,
        int recordNumber,
        byte definitionIndex,
        ushort sequenceNumber,
        ReadOnlyMemory<byte> content)
    {
        byte[] payloadData = new byte[12 + content.Length];
        var span = payloadData.AsSpan();

        BinaryPrimitives.WriteInt32BigEndian(span[0..], tableNumber);
        span[4] = (byte)RecordPayloadType.Memo;
        BinaryPrimitives.WriteInt32BigEndian(span[5..], recordNumber);
        span[9] = definitionIndex;
        BinaryPrimitives.WriteUInt16BigEndian(span[10..], sequenceNumber);
        content.Span.CopyTo(span[12..]);

        return new MemoRecordPayload { PayloadData = payloadData };
    }
}
