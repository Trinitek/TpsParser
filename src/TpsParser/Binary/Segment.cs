using System;
using System.Buffers;

namespace TpsParser.Binary;

internal sealed class Segment : ReadOnlySequenceSegment<byte>
{
    public Segment(Memory<byte> memory)
    {
        Memory = memory;
    }

    public Segment Append(Memory<byte> memory)
    {
        var segment = new Segment(memory)
        {
            RunningIndex = RunningIndex + Memory.Length
        };

        Next = segment;

        return segment;
    }

    public static ReadOnlySequence<byte> CreateSequence(params Memory<byte>[] memories)
    {
        if (memories is null || memories.Length == 0)
        {
            return ReadOnlySequence<byte>.Empty;
        }

        var first = new Segment(memories[0]);
        var current = first;

        for (int i = 1; i < memories.Length; i++)
        {
            current = current.Append(memories[i]);
        }

        return new ReadOnlySequence<byte>(first, 0, current, current.Memory.Length);
    }
}
