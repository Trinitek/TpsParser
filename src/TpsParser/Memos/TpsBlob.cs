using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace TpsParser;

/// <summary>
/// <para>
/// </para>
/// Represents a Clarion <c>BLOB</c>, which is a large variable-length body of binary data.
/// <para>
/// A <see cref="TpsBlob"/> follows a <see cref="MemoDefinition"/> where
/// <see cref="MemoDefinition.IsBlob"/> is <see langword="true"/>.
/// </para>
/// <para>
/// A <see cref="TpsBlob"/> logically represents a single body of data that is assembled from one or more
/// <see cref="TpsRecord"/> objects with a <see cref="MemoRecordPayload"/>, assembled in sequence according
/// to the <see cref="MemoRecordPayload.SequenceNumber"/>.
/// </para>
/// </summary>
public sealed class TpsBlob : ITpsMemo
{
    /// <summary>
    /// The collection of <c>BLOB</c> payloads that make up this <see cref="TpsBlob"/>,
    /// ordered by their <see cref="MemoRecordPayload.SequenceNumber"/>.
    /// </summary>
    public required ImmutableArray<MemoRecordPayload> MemoPayloads { get; init; }

    /// <inheritdoc cref="MemoRecordPayload.TableNumber"/>
    public int TableNumber => MemoPayloads[0].TableNumber;

    /// <inheritdoc cref="MemoRecordPayload.RecordNumber"/>
    public int RecordNumber => MemoPayloads[0].RecordNumber;

    /// <inheritdoc cref="MemoRecordPayload.DefinitionIndex"/>
    public int DefinitionIndex => MemoPayloads[0].DefinitionIndex;

    /// <summary>
    /// Gets the total length in bytes of the combined blob content segments.
    /// </summary>
    public int Length => MemoPayloads.Sum(mp => 
    {
        int length = BinaryPrimitives.ReadInt32LittleEndian(mp.Content.Span);
        return length;
    });

    /// <summary>
    /// Gets the binary content segments of the <c>BLOB</c> from each <see cref="MemoRecordPayload"/>
    /// in <see cref="MemoPayloads"/>.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ReadOnlyMemory<byte>> GetBlobContentSegments()
    {
        foreach (var payload in MemoPayloads)
        {
            int length = BinaryPrimitives.ReadInt32LittleEndian(payload.Content.Span);

            yield return payload.Content[..length];
        }
    }

    /// <inheritdoc cref="ITpsMemo.CopyTo(Span{byte})" />
    public void CopyTo(Span<byte> destination)
    {
        int expectedLength = Length;

        if (destination.Length < expectedLength)
        {
            throw new ArgumentException($"Destination span length {destination.Length} is shorter than the BLOB content length {expectedLength}.", nameof(destination));
        }

        int offset = 0;

        foreach (var contentSegment in GetBlobContentSegments())
        {
            contentSegment.Span.CopyTo(destination[offset..]);
            offset += contentSegment.Length;
        }
    }

    /// <inheritdoc cref="ITpsMemo.ToArray" />
    public byte[] ToArray()
    {
        byte[] result = new byte[Length];

        CopyTo(result);

        return result;
    }
}
