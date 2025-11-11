using System;
using System.Buffers.Binary;

namespace TpsParser;

public readonly record struct DataRecordPayload : IRecordPayload, IPayloadTableNumber, IPayloadRecordNumber
{
    public required ReadOnlyMemory<byte> PayloadData { get; init; }

    /// <inheritdoc cref="IPayloadTableNumber.TableNumber"/>
    public int TableNumber => BinaryPrimitives.ReadInt32BigEndian(PayloadData.Span[0..]);

    /// <inheritdoc cref="IPayloadRecordNumber.RecordNumber"/>
    public int RecordNumber => BinaryPrimitives.ReadInt32BigEndian(PayloadData.Span[5..]);

    /// <summary>
    /// Gets the memory region of the content in this entry.
    /// </summary>
    /// <remarks>
    /// Reverse-engineering note: these can be somewhat large compared to the other record types.
    /// You should expect the length of this to be equal to <see cref="TableDefinition.RecordLength"/>.
    /// </remarks>
    public ReadOnlyMemory<byte> Content => PayloadData[9..];
}
