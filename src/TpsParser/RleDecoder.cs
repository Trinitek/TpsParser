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
        long writtenBytes = 0;
        var unpackedBytes = new List<byte>(expectedUnpackedSize);

        var arrayPool = ArrayPool<byte>.Shared;

        byte[] repeatBuffer = arrayPool.Rent(minimumLength: 0x7FFF /* 32767 */);

        int readPosition = 0;

        try
        {
            do
            {
                int skip = packed[readPosition++];

                if (skip == 0)
                {
                    if (errorHandlingOptions.ThrowOnRleDecompressionError)
                    {
                        throw new RunLengthEncodingException($"Bad RLE Skip (0x00) at position {readPosition - 1}.");
                    }

                    unpacked = null;
                    return false;
                }

                if (skip > 0x7F)
                {
                    int msb = packed[readPosition++];
                    int lsb = skip & 0x7F;
                    int shift = 0x80 * (msb & 0x01);
                    skip = (msb << 7 & 0xFF00) + lsb + shift;
                }

                var spanToAdd = packed.Slice(readPosition, skip);

                writtenBytes += spanToAdd.Length;

                if (writtenBytes > expectedUnpackedSize)
                {
                    if (errorHandlingOptions.RleOversizedDecompressionBehavior == RleSizeMismatchBehavior.Throw)
                    {
                        throw new RunLengthEncodingException($"RLE unpack exceeded expected total size {expectedUnpackedSize}. Exceeded at position {readPosition - 1}, skip {skip}.");
                    }
                    else if (errorHandlingOptions.RleOversizedDecompressionBehavior == RleSizeMismatchBehavior.Skip)
                    {
                        unpacked = null;
                        return false;
                    }
                }

                unpackedBytes.AddRange(spanToAdd);
                readPosition += skip;

                if (readPosition < packed.Length)
                {
                    readPosition--;

                    byte toRepeat = packed[readPosition++];
                    int repeatsMinusOne = packed[readPosition++];

                    if (repeatsMinusOne > 0x7F)
                    {
                        int msb = packed[readPosition++];
                        int lsb = repeatsMinusOne & 0x7F;
                        int shift = 0x80 * (msb & 0x01);
                        repeatsMinusOne = (msb << 7 & 0xFF00) + lsb + shift;

                        // Repeats can be up to 0x7FFF (32767) bytes
                    }

                    var repeatSpan = repeatBuffer.AsSpan(start: 0, length: repeatsMinusOne);

                    writtenBytes += repeatSpan.Length;

                    if (writtenBytes > expectedUnpackedSize)
                    {
                        if (errorHandlingOptions.RleOversizedDecompressionBehavior == RleSizeMismatchBehavior.Throw)
                        {
                            throw new RunLengthEncodingException($"RLE unpack exceeded expected total size {expectedUnpackedSize}. Exceeded at position {readPosition - 1}, repeat {repeatsMinusOne}.");
                        }
                        else if (errorHandlingOptions.RleOversizedDecompressionBehavior == RleSizeMismatchBehavior.Skip)
                        {
                            unpacked = null;
                            return false;
                        }
                    }

                    repeatSpan.Fill(toRepeat);

                    unpackedBytes.AddRange(repeatSpan);
                }
            }
            while (readPosition < packed.Length - 1);
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

        unpacked = [.. unpackedBytes];
        return true;
    }
}
