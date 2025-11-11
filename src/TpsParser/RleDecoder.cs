using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace TpsParser;

public static class RleDecoder
{
    public static byte[] Unpack(
        ReadOnlySpan<byte> packed,
        int expectedUnpackedSize,
        Encoding encoding,
        ErrorHandlingOptions errorHandlingOptions)
    {
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
                    throw new RunLengthEncodingException("Bad RLE Skip (0x00)");
                }

                if (skip > 0x7F)
                {
                    int msb = packed[position++];
                    int lsb = skip & 0x7F;
                    int shift = 0x80 * (msb & 0x01);
                    skip = (msb << 7 & 0xFF00) + lsb + shift;
                }

                unpackedBytes.AddRange(packed[position..(position + skip)]);
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

        //return new TpsRandomAccess([.. unpackedBytes], encoding);
        return [.. unpackedBytes];
    }
}
