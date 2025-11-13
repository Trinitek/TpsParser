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
    /// For <c>BLOB</c>s, the first 4 bytes is a little-endian 32-bit integer describing the size of
    /// the blob content, followed by the content. It's unclear if the reported size differs from
    /// the implied size based on <see cref="TpsRecord.PayloadTotalLength"/>, or if it could be shorter.
    /// </remarks>
    public readonly ReadOnlyMemory<byte> Content => PayloadData[12..];
}
