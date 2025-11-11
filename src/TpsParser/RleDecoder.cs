using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TpsParser;

/// <summary></summary>
public static class RleDecoder
{
    /// <summary>
    /// Unpacks a run-length encoded sequence of bytes into a new byte array.
    /// </summary>
    /// <param name="packed"></param>
    /// <param name="expectedUnpackedSize"></param>
    /// <param name="errorHandlingOptions"></param>
    /// <param name="unpacked"></param>
    /// <returns></returns>
    /// <exception cref="RunLengthEncodingException"></exception>
    public static bool TryUnpack(
        ReadOnlySpan<byte> packed,
        int expectedUnpackedSize,
        ErrorHandlingOptions errorHandlingOptions,
        [NotNullWhen(true)] out byte[]? unpacked)
    {
        // TODO need to detect buffer overruns conditional on the error handling options

        var unpackedBytes = new List<byte>(expectedUnpackedSize);

        var arrayPool = ArrayPool<byte>.Shared;

        byte[] repeatBuffer = arrayPool.Rent(minimumLength: 0x7FFF /* 32767 */);

        int position = 0;

        try
        {
            do
            {
                int skip = packed[position++];

                if (skip == 0)
                {
                    if (errorHandlingOptions.ThrowOnRleDecompressionError)
                    {
                        throw new RunLengthEncodingException($"Bad RLE Skip (0x00) at position {position - 1}.");
                    }

                    unpacked = null;
                    return false;
                }

                if (skip > 0x7F)
                {
                    int msb = packed[position++];
                    int lsb = skip & 0x7F;
                    int shift = 0x80 * (msb & 0x01);
                    skip = (msb << 7 & 0xFF00) + lsb + shift;
                }

                unpackedBytes.AddRange(packed.Slice(position, skip));
                position += skip;

                if (position < packed.Length)
                {
                    position--;

                    byte toRepeat = packed[position++];
                    int repeatsMinusOne = packed[position++];

                    if (repeatsMinusOne > 0x7F)
                    {
                        int msb = packed[position++];
                        int lsb = repeatsMinusOne & 0x7F;
                        int shift = 0x80 * (msb & 0x01);
                        repeatsMinusOne = (msb << 7 & 0xFF00) + lsb + shift;

                        // Repeats can be up to 0x7FFF (32767) bytes
                    }

                    var repeatSpan = repeatBuffer.AsSpan(start: 0, length: repeatsMinusOne);

                    repeatSpan.Fill(toRepeat);

                    unpackedBytes.AddRange(repeatSpan);
                }
            }
            while (position < packed.Length - 1);
        }
        finally
        {
            arrayPool.Return(repeatBuffer);
        }

        if (unpackedBytes.Count < expectedUnpackedSize)
        {
            if (errorHandlingOptions.RleUndersizedDecompressionBehavior == RleSizeMismatchBehavior.Throw)
            {
                throw new RunLengthEncodingException($"RLE unpacked size mismatch (expected {expectedUnpackedSize}, got {unpackedBytes.Count}).");
            }
            else if (errorHandlingOptions.RleUndersizedDecompressionBehavior == RleSizeMismatchBehavior.Skip)
            {
                unpacked = null;
                return false;
            }
        }

        if (unpackedBytes.Count > expectedUnpackedSize)
        {
            if (errorHandlingOptions.RleOversizedDecompressionBehavior == RleSizeMismatchBehavior.Throw)
            {
                throw new RunLengthEncodingException($"RLE unpacked size mismatch (expected {expectedUnpackedSize}, got {unpackedBytes.Count}).");
            }
            else if (errorHandlingOptions.RleOversizedDecompressionBehavior == RleSizeMismatchBehavior.Skip)
            {
                unpacked = null;
                return false;
            }
        }

        unpacked = [.. unpackedBytes];
        return true;
    }
}
