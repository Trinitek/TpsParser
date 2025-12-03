using System;
using System.Collections.Immutable;

namespace TpsParser;

/// <summary>
/// Represents a Clarion <c>MEMO</c> or <c>BLOB</c>, which are large variable-length bodies of data or text.
/// </summary>
public interface ITpsMemo
{
    /// <summary>
    /// The collection of <c>MEMO</c> or <b>BLOB</b> payloads that make up this <see cref="ITpsMemo"/>,
    /// ordered by their <see cref="MemoRecordPayload.SequenceNumber"/>.
    /// </summary>
    ImmutableArray<MemoRecordPayload> MemoPayloads { get; }

    /// <inheritdoc cref="MemoRecordPayload.TableNumber"/>
    int TableNumber { get; }

    /// <inheritdoc cref="MemoRecordPayload.RecordNumber"/>
    int RecordNumber { get; }

    /// <inheritdoc cref="MemoRecordPayload.DefinitionIndex"/>
    int DefinitionIndex { get; }

    /// <summary>
    /// Gets the total length in bytes of the combined <see cref="MemoPayloads"/> contents.
    /// </summary>
    int Length { get; }

    /// <summary>
    /// Copies the byte contents into the provided <see cref="Span{Byte}"/>.
    /// </summary>
    /// <param name="destination"></param>
    /// <exception cref="ArgumentException">
    /// <paramref name="destination"/> is shorter than the content length <see cref="Length"/>.
    /// </exception>
    void CopyTo(Span<byte> destination);

    /// <summary>
    /// Copies the byte contents into a new array.
    /// </summary>
    /// <returns></returns>
    byte[] ToArray();
}
