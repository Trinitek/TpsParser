using System;
using System.Collections.Generic;
using System.Linq;

namespace TpsParser;

/// <summary>
/// Contains static methods for constructing <see cref="ITpsMemo"/> instances from payloads.
/// </summary>
public static class TpsMemoBuilder
{
    /// <summary>
    /// Builds an enumerable of <see cref="ITpsMemo"/> objects from an enumerable of <see cref="MemoRecordPayload"/> objects,
    /// joined and merged based on the payloads' definition index, record number, and sequence number.
    /// </summary>
    /// <param name="memoPayloads"></param>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IEnumerable<T> BuildTpsMemos<T>(
        IEnumerable<MemoRecordPayload> memoPayloads,
        Func<IEnumerable<MemoRecordPayload>, T> builder)
        where T : ITpsMemo
    {
        // Group the records by their owner and index.
        var groupedByOwnerAndIndex = memoPayloads
            .GroupBy(
                keySelector: record =>
                {
                    return (owner: record.RecordNumber, index: record.DefinitionIndex);
                },
                // Records must be merged in order according to the memo's sequence number, so we order them.
                // Large memos are spread across multiple structures and must be joined later.
                resultSelector: (key, payloads) =>
                    (key,
                    payloads: payloads.OrderBy(payload => payload.SequenceNumber)));

        // Drop memos that have skipped sequence numbers, as this means the memo is missing a chunk of data.
        // Sequence numbers are zero-based.
        var filteredByCompleteSequences = groupedByOwnerAndIndex
            .Where(group => group.payloads.Count() - 1 == group.payloads.Last().SequenceNumber);

        // Merge memo sequences into a single memo record.
        var resultingMemoRecords = filteredByCompleteSequences
            .Select(group =>
            {
                var mergedMemo = builder.Invoke(group.payloads);

                return mergedMemo;
            });

        return resultingMemoRecords;
    }

    /// <summary>
    /// Builds an enumerable of <see cref="ITpsMemo"/> objects from an enumerable of <see cref="MemoRecordPayload"/> objects,
    /// joined and merged based on the payloads' definition index, record number, and sequence number.
    /// </summary>
    /// <param name="memoPayloads"></param>
    /// <param name="tableDefinition"></param>
    /// <returns></returns>
    public static IEnumerable<ITpsMemo> BuildTpsMemos(
        IEnumerable<MemoRecordPayload> memoPayloads,
        TableDefinition tableDefinition)
    {
        return BuildTpsMemos<ITpsMemo>(
            memoPayloads: memoPayloads,
            builder: payloads =>
            {
                var first = payloads.First();

                var def = tableDefinition.Memos[first.DefinitionIndex];

                if (def.IsTextMemo)
                {
                    return new TpsTextMemo
                    {
                        MemoPayloads = [.. payloads]
                    };
                }
                else
                {
                    return new TpsBlob
                    {
                        MemoPayloads = [.. payloads]
                    };
                }
            });
    }

    /// <summary>
    /// Builds an enumerable of <see cref="TpsBlob"/> objects from an enumerable of <see cref="MemoRecordPayload"/> objects,
    /// joined and merged based on the payloads' definition index, record number, and sequence number.
    /// All record payloads are assumed to describe <c>BLOB</c> payloads.
    /// The inclusion of <c>MEMO</c> payloads will produce incorrectly-formed <see cref="TpsBlob"/> objects.
    /// </summary>
    /// <param name="blobPayloads"></param>
    /// <returns></returns>
    public static IEnumerable<TpsBlob> BuildTpsBlobs(IEnumerable<MemoRecordPayload> blobPayloads)
    {
        return BuildTpsMemos(
            memoPayloads: blobPayloads,
            builder: payloads =>
            {
                var blob = new TpsBlob
                {
                    MemoPayloads = [.. payloads]
                };
                return blob;
            });
    }

    /// <summary>
    /// Builds an enumerable of <see cref="TpsTextMemo"/> objects from an enumerable of <see cref="MemoRecordPayload"/> objects,
    /// joined and merged based on the payloads' definition index, record number, and sequence number.
    /// All record payloads are assumed to describe <c>MEMO</c> payloads.
    /// The inclusion of <c>BLOB</c> payloads will produce incorrectly-formed <see cref="TpsTextMemo"/> objects.
    /// </summary>
    /// <param name="textMemoPayloads"></param>
    /// <returns></returns>
    public static IEnumerable<TpsTextMemo> BuildTpsTextMemos(IEnumerable<MemoRecordPayload> textMemoPayloads)
    {
        return BuildTpsMemos(
            memoPayloads: textMemoPayloads,
            builder: payloads =>
            {
                var blob = new TpsTextMemo
                {
                    MemoPayloads = [.. payloads]
                };
                return blob;
            });
    }
}
