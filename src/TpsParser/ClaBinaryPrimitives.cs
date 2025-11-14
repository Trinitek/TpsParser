using System;
using System.Buffers.Binary;
using TpsParser.TypeModel;

namespace TpsParser;

public static class ClaBinaryPrimitives
{
    public static ClaByte ReadClaByte(ReadOnlySpan<byte> source)
    {
        return new ClaByte(source[0]);
    }

    public static ClaShort ReadClaShort(ReadOnlySpan<byte> source)
    {
        return new ClaShort(BinaryPrimitives.ReadInt16LittleEndian(source));
    }

    public static ClaUnsignedShort ReadClaUnsignedShort(ReadOnlySpan<byte> source)
    {
        return new ClaUnsignedShort(BinaryPrimitives.ReadUInt16LittleEndian(source));
    }

    public static ClaLong ReadClaLong(ReadOnlySpan<byte> source)
    {
        return new ClaLong(BinaryPrimitives.ReadInt32LittleEndian(source));
    }

    public static ClaUnsignedLong ReadClaUnsignedLong(ReadOnlySpan<byte> source)
    {
        return new ClaUnsignedLong(BinaryPrimitives.ReadUInt32LittleEndian(source));
    }

    public static ClaDate ReadClaDate(ReadOnlySpan<byte> source)
    {
        uint date = BinaryPrimitives.ReadUInt32LittleEndian(source);

        if (date != 0)
        {
            long years = (date & 0xFFFF0000) >> 16;
            long months = (date & 0x0000FF00) >> 8;
            long days = date & 0x000000FF;
            return new ClaDate(new DateOnly((int)years, (int)months, (int)days));
        }
        else
        {
            return new ClaDate(null);
        }
    }

    public static ClaTime ReadClaTime(ReadOnlySpan<byte> source)
    {
        uint time = BinaryPrimitives.ReadUInt32LittleEndian(source);

        // Hours 0 - 23
        uint hours = (time & 0x7F000000) >> 24;

        // Minutes 0 - 59
        uint mins = (time & 0x00FF0000) >> 16;

        // Seconds 0 - 59
        uint secs = (time & 0x0000FF00) >> 8;

        // Centiseconds (seconds/100) 0 - 99
        uint centi = time & 0x000000FF;

        return new ClaTime((byte)hours, (byte)mins, (byte)secs, (byte)centi);
    }

    public static ClaSingleReal ReadClaSingleReal(ReadOnlySpan<byte> source)
    {
        return new ClaSingleReal(BinaryPrimitives.ReadSingleLittleEndian(source));
    }

    public static ClaReal ReadClaReal(ReadOnlySpan<byte> source)
    {
        return new ClaReal(BinaryPrimitives.ReadDoubleLittleEndian(source));
    }

    public static ClaDecimal ReadClaDecimal(ReadOnlySpan<byte> source, int length, byte digitsAfterDecimalPoint)
    {
        if (length < 1 || length > 16)
        {
            throw new ArgumentOutOfRangeException(nameof(length), actualValue: length, "Expected a byte length between 1 and 16 inclusive.");
        }

        ulong high = default;
        ulong low = default;
        byte places = digitsAfterDecimalPoint;

        ref ulong current = ref length > 16 ? ref high : ref low;

        ReadOnlySpan<byte> data = source[..length];

        int shift = 0;

        // Write the least significant 30 digits
        for (int i = length - 1; i > 0; i--)
        {
            current |= (ulong)data[i] << 8 * shift;

            if (i == 16)
            {
                shift = 0;
                current = ref high;
            }
            else
            {
                shift++;
            }
        }

        // Most significant digit (may be zero)
        current |= ((ulong)data[0] & 0x0F) << 8 * shift;

        // Sign
        high |= ((ulong)data[0] & 0xF0) << 56;

        return new ClaDecimal(high, low, places);
    }
}
