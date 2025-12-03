using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace TpsParser;

/// <summary>
/// <para>
/// Represents a Clarion <c>MEMO</c>, which is a large variable-length body of text.
/// </para>
/// <para>
/// A <see cref="TpsTextMemo"/> follows a <see cref="MemoDefinition"/> where
/// <see cref="MemoDefinition.IsTextMemo"/> is <see langword="true"/>.
/// </para>
/// <para>
/// A <see cref="TpsTextMemo"/> logically represents a single body of text that is assembled from one or more
/// <see cref="TpsRecord"/> objects with a <see cref="MemoRecordPayload"/>, assembled in sequence according
/// to the <see cref="MemoRecordPayload.SequenceNumber"/>.
/// </para>
/// </summary>
public sealed class TpsTextMemo : ITpsMemo
{
    /// <summary>
    /// The collection of <c>MEMO</c> payloads that make up this <see cref="TpsTextMemo"/>,
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
    /// Gets the total length in bytes of the combined <see cref="MemoPayloads"/> contents.
    /// </summary>
    public int Length => MemoPayloads.Sum(mp => mp.Content.Length);

    /// <summary>
    /// Decodes the byte contents into a <see cref="string"/> using the specified <see cref="Encoding"/>.
    /// </summary>
    /// <param name="encoding"></param>
    /// <returns></returns>
    // Adapted from .NET Runtime EncodingExtensions.ToString implementation, but instead of working with
    // ReadOnlySequence<byte>, works with an array of ReadOnlyMemory<byte>.
    // https://github.com/dotnet/runtime/blob/f05acbbb1c4ca1ca8f4def64e9449c4d99d8af21/src/libraries/System.Memory/src/System/Text/EncodingExtensions.cs#L325-L398
    public string ToString(Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(encoding);

        if (MemoPayloads.Length == 1)
        {
            // If the incoming sequence is single-segment, one-shot this.

            return encoding.GetString(MemoPayloads[0].Content.Span);
        };

        // Otherwise, if the incoming sequence is multi-segment, create a stateful Decoder
        // and use it as the workhorse. On the final iteration we'll pass flush=true.

        Decoder decoder = encoding.GetDecoder();

        // Maintain a list of all the segments we'll need to concat together.
        // These will be released back to the pool at the end of the method.

        List<(char[], int)> listOfSegments = new(MemoPayloads.Length);
        
        int totalCharCount = 0;

        for (int i = 0; i < MemoPayloads.Length; i++)
        {
            var currentContent = MemoPayloads[i].Content.Span;
            bool isFinalContent = i == MemoPayloads.Length - 1;

            int charCountThisIteration = decoder.GetCharCount(
                bytes: currentContent,
                flush: isFinalContent);

            char[] rentedArray = ArrayPool<char>.Shared.Rent(minimumLength: charCountThisIteration);

            int actualCharsWrittenThisIteration = decoder.GetChars(
                bytes: currentContent,
                chars: rentedArray,
                flush: isFinalContent);

            listOfSegments.Add((rentedArray, actualCharsWrittenThisIteration));

            totalCharCount += actualCharsWrittenThisIteration;

            if (totalCharCount < 0)
            {
                // If we overflowed, call string.Create, passing int.MaxValue.
                // This will end up throwing the expected OutOfMemoryException
                // since strings are limited to under int.MaxValue elements in length.

                totalCharCount = int.MaxValue;
                break;
            }
        }

        // Now build up the string to return, then release all of our scratch buffers
        // back to the shared pool.

        string newString = string.Create(
            length: totalCharCount,
            state: listOfSegments,
            action: static (span, listOfSegments) =>
            {
                foreach ((char[] array, int length) in listOfSegments)
                {
                    array.AsSpan(0, length).CopyTo(span);
                    
                    ArrayPool<char>.Shared.Return(array);

                    span = span[length..];
                }

                Debug.Assert(span.IsEmpty, "Over-allocated the string instance?");
            });

        return newString;
    }

    /// <summary>
    /// Decodes the byte contents into a <see cref="string"/> using the default
    /// <see cref="EncodingOptions.ContentEncoding"/> specified in <see cref="EncodingOptions.Default"/>.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => ToString(EncodingOptions.Default.ContentEncoding);

    /// <inheritdoc cref="ITpsMemo.CopyTo(Span{byte})" />
    public void CopyTo(Span<byte> destination)
    {
        int expectedLength = Length;

        if (destination.Length < expectedLength)
        {
            throw new ArgumentException($"Destination span length {destination.Length} is shorter than the MEMO content length {expectedLength}.", nameof(destination));
        }

        int offset = 0;

        foreach (var payload in MemoPayloads)
        {
            payload.Content.Span.CopyTo(destination[offset..]);
            offset += payload.Content.Length;
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
