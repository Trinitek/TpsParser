using System;
using System.Buffers.Binary;

namespace TpsParser;

public readonly record struct TableDefinitionRecordPayload : IRecordPayload, IPayloadTableNumber
{
    public required ReadOnlyMemory<byte> PayloadData { get; init; }

    /// <inheritdoc cref="IPayloadTableNumber.TableNumber"/>
    public int TableNumber => BinaryPrimitives.ReadInt32BigEndian(PayloadData.Span[0..]);

    /// <summary>
    /// Gets the sequence number of the <see cref="TableDefinition"/> when the definition is segmented into multiple records. The first segment is 0.
    /// </summary>
    public ushort SequenceNumber => BinaryPrimitives.ReadUInt16LittleEndian(PayloadData.Span[5..]);

    /// <summary>
    /// Gets the memory region of the content in this entry.
    /// </summary>
    /// <remarks>
    /// Reverse-engineering note: this seems to be at most 512 bytes.
    /// </remarks>
    public ReadOnlyMemory<byte> Content => PayloadData[7..];
}
