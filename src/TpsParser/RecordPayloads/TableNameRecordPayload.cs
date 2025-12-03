using System;
using System.Buffers.Binary;
using System.Text;

namespace TpsParser;

/// <summary>
/// A record payload that contains the name of a table.
/// </summary>
public sealed record TableNameRecordPayload : IRecordPayload, IPayloadTableNumber
{
    /// <inheritdoc/>
    public required ReadOnlyMemory<byte> PayloadData { get; init; }

    /// <summary></summary>
    public required ushort PayloadHeaderLength { get; init; }

    /// <inheritdoc cref="IPayloadTableNumber.TableNumber"/>
    public int TableNumber => BinaryPrimitives.ReadInt32BigEndian(PayloadData.Span[PayloadHeaderLength..]);

    /// <summary>
    /// Gets the name of the table.
    /// </summary>
    public string GetName(Encoding encoding) => encoding.GetString(PayloadData.Span[1..PayloadHeaderLength]);
}
