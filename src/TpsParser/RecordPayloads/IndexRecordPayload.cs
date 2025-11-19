using System;
using System.Buffers.Binary;

namespace TpsParser;

/// <summary>
/// A record payload that contains information about an index.
/// </summary>
public readonly record struct IndexRecordPayload : IRecordPayload, IPayloadTableNumber, IPayloadRecordNumber
{
    /// <inheritdoc/>
    public required ReadOnlyMemory<byte> PayloadData { get; init; }

    /// <inheritdoc cref="IPayloadTableNumber.TableNumber"/>
    public int TableNumber => BinaryPrimitives.ReadInt32BigEndian(PayloadData.Span[0..]);

    /// <summary>
    /// Gets the index number of the corresponding definition in <see cref="TableDefinition.Indexes"/>.
    /// </summary>
    public byte DefinitionIndex => PayloadData.Span[4];

    /// <summary>
    /// Gets the number of the <see cref="TpsRecord"/> with payload type <see cref="DataRecordPayload"/> to which this index belongs.
    /// </summary>
    public int RecordNumber => BinaryPrimitives.ReadInt32BigEndian(PayloadData.Span[^4..]);
}
