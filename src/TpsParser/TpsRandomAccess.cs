using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;
using TpsParser.TypeModel;

namespace TpsParser;

/// <summary>
/// A binary reader for format-specific types and structures.
/// </summary>
public sealed class TpsRandomAccess
{
    private byte[] Data { get; }

    /// <summary>
    /// Gets the default encoding used when reading strings.
    /// </summary>
    public Encoding Encoding { get; }

    /// <summary>
    /// Gets the base offset position in the data array.
    /// </summary>
    public int BaseOffset { get; }

    /// <summary>
    /// Gets the current position in the data array relative to <see cref="BaseOffset"/>.
    /// </summary>
    public int Position { get; private set; }

    /// <summary>
    /// Gets the absolute position in the data array.
    /// </summary>
    public int AbsolutePosition => BaseOffset + Position;

    /// <summary>
    /// Gets the length of the data array.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Returns true if there is one byte available at <see cref="Position"/>.
    /// </summary>
    public bool IsOneByteLeft => Position > Length - 1;

    /// <summary>
    /// Returns true if no more data is available at <see cref="Position"/>.
    /// </summary>
    public bool IsAtEnd => Position >= Length - 1;

    /// <summary>
    /// Instantiates a new reader from a byte array.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="encoding"></param>
    public TpsRandomAccess(byte[] data, Encoding encoding)
        : this(
              data: data,
              baseOffset: 0,
              length: data?.Length ?? throw new ArgumentNullException(nameof(data)),
              encoding: encoding)
    { }

    /// <summary>
    /// Instantiates a new reader from a byte array.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="baseOffset"></param>
    /// <param name="length"></param>
    /// <param name="encoding"></param>
    public TpsRandomAccess(byte[] data, int baseOffset, int length, Encoding encoding)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(baseOffset);
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        Position = 0;
        Data = data ?? throw new ArgumentNullException(nameof(data));
        BaseOffset = baseOffset;
        Length = length;

        Encoding = encoding;
    }

    /// <summary>
    /// Instantiates a new reader from an existing one.
    /// </summary>
    /// <param name="existing"></param>
    /// <param name="additiveOffset"></param>
    /// <param name="length"></param>
    /// <param name="encoding">The encoding to use, or null if the encoding should be inherited from <paramref name="encoding"/>.</param>
    public TpsRandomAccess(TpsRandomAccess existing, int additiveOffset, int length, Encoding? encoding = null)
        : this(
              data: existing?.Data ?? throw new ArgumentNullException(nameof(existing)),
              baseOffset: existing.BaseOffset + additiveOffset,
              length: length,
              encoding: encoding ?? existing.Encoding)
    { }

    private void AssertSpace(int numberOfBytes)
    {
        if (Position + numberOfBytes > Length)
        {
            throw new IndexOutOfRangeException($"Data type of size {numberOfBytes} exceeds the end of the data array at offset {Position} by {Position - Length + numberOfBytes}. Array is {Length} bytes long.");
        }
        if (Position < 0)
        {
            throw new IndexOutOfRangeException($"The offset ({Position}) should not be negative.");
        }
    }

    /// <summary>
    /// Reads a little endian 2s-complement signed 4 byte integer.
    /// </summary>
    /// <returns></returns>
    public int PeekLongLE()
    {
        AssertSpace(4);

        int result = BinaryPrimitives.ReadInt32LittleEndian(Data.AsSpan(AbsolutePosition));

        return result;
    }

    /// <summary>
    /// Reads a little endian 2s-complement signed 4 byte integer and advances the current position.
    /// </summary>
    /// <returns></returns>
    public int ReadLongLE()
    {
        int result = PeekLongLE();

        Position += 4;
        return result;
    }

    /// <summary>
    /// Writes a 4 byte little endian integer and advances the current position. This is typically used when decrypting.
    /// </summary>
    /// <param name="value"></param>
    public void WriteLongLE(int value)
    {
        AssertSpace(4);

        BinaryPrimitives.WriteInt32LittleEndian(Data.AsSpan(AbsolutePosition), value);

        Position += 4;
    }

    /// <summary>
    /// Reads a little endian unsigned 4 byte integer.
    /// </summary>
    /// <returns></returns>
    public uint ReadUnsignedLongLE()
    {
        AssertSpace(4);

        uint result = BinaryPrimitives.ReadUInt32LittleEndian(Data.AsSpan(AbsolutePosition));

        Position += 4;
        return result;
    }

    /// <summary>
    /// Reads a big endian signed 4 byte integer.
    /// </summary>
    /// <returns></returns>
    public int PeekLongBE()
    {
        AssertSpace(4);

        int result = BinaryPrimitives.ReadInt32BigEndian(Data.AsSpan(AbsolutePosition));

        return result;
    }

    /// <summary>
    /// Reads a big endian signed 4 byte integer and advances the current position.
    /// </summary>
    /// <returns></returns>
    public int ReadLongBE()
    {
        int result = PeekLongBE();

        Position += 4;
        return result;
    }

    /// <summary>
    /// Reads a big endian unsigned integer and advances the current position.
    /// </summary>
    /// <returns></returns>
    public uint ReadUnsignedLongBE()
    {
        AssertSpace(4);

        uint result = BinaryPrimitives.ReadUInt32BigEndian(Data.AsSpan(AbsolutePosition));

        Position += 4;
        return result;
    }

    /// <summary>
    /// Reads a little endian signed short and advances the current position.
    /// </summary>
    /// <returns></returns>
    public short ReadShortLE()
    {
        AssertSpace(2);

        short result = BinaryPrimitives.ReadInt16LittleEndian(Data.AsSpan(AbsolutePosition));

        Position += 2;

        return result;
    }

    /// <summary>
    /// Reads a little endian unsigned short and advances the current position.
    /// </summary>
    /// <returns></returns>
    public ushort ReadUnsignedShortLE()
    {
        AssertSpace(2);

        ushort result = BinaryPrimitives.ReadUInt16LittleEndian(Data.AsSpan(AbsolutePosition));

        Position += 2;

        return result;
    }

    /// <summary>
    /// Reads a big endian signed short and advances the current position.
    /// </summary>
    /// <returns></returns>
    public short ReadShortBE()
    {
        AssertSpace(2);

        short result = BinaryPrimitives.ReadInt16BigEndian(Data.AsSpan(AbsolutePosition));

        Position += 2;

        return result;
    }

    /// <summary>
    /// Reads a big endian unsigned short and advances the current position.
    /// </summary>
    /// <returns></returns>
    public ushort ReadUnsignedShortBE()
    {
        AssertSpace(2);

        ushort result = BinaryPrimitives.ReadUInt16BigEndian(Data.AsSpan(AbsolutePosition));

        Position += 2;

        return result;
    }

    /// <summary>
    /// Reads a byte and advances the current position.
    /// </summary>
    /// <returns></returns>
    public byte ReadByte()
    {
        AssertSpace(1);

        byte result = Data[AbsolutePosition];

        Position += 1;

        return result;
    }

    /// <summary>
    /// Reads a byte from the given offset without advancing the position.
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public byte PeekByte(int offset) => Data[BaseOffset + offset];

    /// <summary>
    /// Reads a little endian float and advances the current position.
    /// </summary>
    /// <returns></returns>
    public float ReadFloatLE()
    {
        AssertSpace(4);

        float result = BinaryPrimitives.ReadSingleLittleEndian(Data.AsSpan(AbsolutePosition));

        Position += 4;

        return result;
    }

    /// <summary>
    /// Reads a little endian double and advances the current position.
    /// </summary>
    /// <returns></returns>
    public double ReadDoubleLE()
    {
        AssertSpace(8);

        double result = BinaryPrimitives.ReadDoubleLittleEndian(Data.AsSpan(AbsolutePosition));

        Position += 8;

        return result;
    }

    /// <summary>
    /// Reads a fixed length string and advances the current position.
    /// </summary>
    /// <param name="length">The length of the string to read.</param>
    /// <param name="encoding">The encoding of the string.</param>
    /// <returns></returns>
    public string ReadFixedLengthString(int length, Encoding? encoding = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        AssertSpace(length);

        encoding ??= Encoding;

        var mem = ReadBytes(length);

        // TODO test coverage to ensure this doesn't eat the entire array.
        string result = encoding.GetString(mem.Span);

        return result;
    }

    /// <summary>
    /// Reads a zero-terminated string and advances the current position.
    /// </summary>
    /// <param name="encoding">The encoding of the string to use.</param>
    /// <returns></returns>
    public string ReadZeroTerminatedString(Encoding? encoding = null)
    {
        var bytes = new List<byte>();

        byte value;

        do
        {
            value = ReadByte();

            if (value != 0)
            {
                bytes.Add(value);
            }
        }
        while (value != 0);

        encoding ??= Encoding;

        return encoding.GetString(bytes.ToArray());
    }

    /// <summary>
    /// Reads a Pascal string and advances the current position. Pascal strings have their length encoded in the first byte.
    /// </summary>
    /// <param name="encoding">The encoding of the string.</param>
    /// <returns></returns>
    public string ReadPascalString(Encoding? encoding = null)
    {
        int length = ReadByte();

        var bytes = new List<byte>();

        for (int i = 0; i < length; i++)
        {
            bytes.Add(ReadByte());
        }

        encoding ??= Encoding;

        return encoding.GetString(bytes.ToArray());
    }

    /// <summary>
    /// Sets the current position to the given offset.
    /// </summary>
    /// <param name="offset">The new offset.</param>
    /// <returns></returns>
    public TpsRandomAccess JumpAbsolute(int offset)
    {
        Position = offset;
        return this;
    }

    /// <summary>
    /// Sets the current position relative to the given offset.
    /// </summary>
    /// <param name="offset">The relative offset.</param>
    /// <returns></returns>
    public TpsRandomAccess JumpRelative(int offset)
    {
        Position += offset;
        return this;
    }

    /// <summary>
    /// Gets the data array. If a base offset and length was specified, then only that section of the array is returned.
    /// </summary>
    /// <returns></returns>
    public byte[] GetData()
    {
        if (BaseOffset == 0
            && Data.Length == Length)
        {
            return Data;
        }
        else
        {
            byte[] dest = new byte[Length];

            Array.Copy(Data, BaseOffset, dest, 0, Length);

            return dest;
        }
    }

    /// <summary>
    /// Gets a span that reflects the data starting from the base offset.
    /// </summary>
    /// <returns></returns>
    public ReadOnlySpan<byte> PeekBaseSpan()
    {
        return new ReadOnlySpan<byte>(array: Data, start: BaseOffset, length: Length);
    }

    /// <summary>
    /// Gets a memory region that reflects the data starting from the current position.
    /// </summary>
    /// <returns></returns>
    public ReadOnlyMemory<byte> PeekRemainingMemory()
    {
        return new ReadOnlyMemory<byte>(array: Data, start: AbsolutePosition, length: Length - Position);
    }

    /// <summary>
    /// Gets a new <see cref="TpsRandomAccess"/> of the given length at the current position and advances the position.
    /// </summary>
    /// <param name="length">The length of the data array.</param>
    /// <returns></returns>
    public TpsRandomAccess Read(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        AssertSpace(length);

        int reference = AbsolutePosition;
        Position += length;

        return new TpsRandomAccess(Data, reference, length, Encoding);
    }

    /// <summary>
    /// Reads a region of bytes and advances the position.
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public ReadOnlyMemory<byte> ReadBytes(int length)
    {
        var rom = PeekBytes(length);

        Position += length;

        return rom;
    }

    /// <summary>
    /// Returns a <see cref="ReadOnlyMemory{T}"/> from the current position.
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public ReadOnlyMemory<byte> PeekBytes(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        AssertSpace(length);

        var rom = new ReadOnlyMemory<byte>(Data, start: AbsolutePosition, length: length);

        return rom;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"0x{Position:X}/0x{Length:X}";
    }

    internal string ToHexString(int step, bool ascii)
    {
        var sb = new StringBuilder();

        for (int i = 0; i < Length; i++)
        {
            sb.Append($"{i:x4} : ");

            for (int y = 0; y < step; y++)
            {
                if (i + y < Length)
                {
                    sb.Append($"{Data[BaseOffset + i + y] & 0xFF:x2} ");
                }
            }

            if (ascii)
            {
                sb.Append(' ');

                for (int y = 0; y < step; y++)
                {
                    if (i + y < Length)
                    {
                        int ch = Data[BaseOffset + i + y] & 0xFF;

                        if (ch < 32 && ch > 127)
                        {
                            sb.Append('.');
                        }
                        else
                        {
                            sb.Append((char)ch);
                        }
                    }
                }
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Reads a <see cref="ClaByte"/> and advances the current position.
    /// </summary>
    /// <returns></returns>
    public ClaByte ReadClaByte() => new(ReadByte());

    /// <summary>
    /// Reads a <see cref="ClaShort"/> and advances the current position.
    /// </summary>
    /// <returns></returns>
    public ClaShort ReadClaShort() => new(ReadShortLE());

    /// <summary>
    /// Reads a <see cref="ClaUnsignedShort"/> and advances the current position.
    /// </summary>
    /// <returns></returns>
    public ClaUnsignedShort ReadClaUnsignedShort() => new(ReadUnsignedShortLE());

    /// <summary>
    /// Reads a <see cref="ClaDate"/> and advances the current position.
    /// </summary>
    /// <returns></returns>
    public ClaDate ReadClaDate()
    {
        long date = ReadUnsignedLongLE();

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

    /// <summary>
    /// Reads a <see cref="ClaTime"/> and advances the current position.
    /// </summary>
    /// <returns></returns>
    public ClaTime ReadClaTime()
    {
        int time = ReadLongLE();

        // Hours 0 - 23
        int hours = (time & 0x7F000000) >> 24;

        // Minutes 0 - 59
        int mins = (time & 0x00FF0000) >> 16;

        // Seconds 0 - 59
        int secs = (time & 0x0000FF00) >> 8;

        // Centiseconds (seconds/100) 0 - 99
        int centi = time & 0x000000FF;

        return new ClaTime((byte)hours, (byte)mins, (byte)secs, (byte)centi);
    }

    /// <summary>
    /// Reads a <see cref="ClaLong"/> and advances the current position.
    /// </summary>
    /// <returns></returns>
    public ClaLong ReadClaLong() => new(ReadLongLE());

    /// <summary>
    /// Reads a <see cref="ClaUnsignedLong"/> and advances the current position.
    /// </summary>
    /// <returns></returns>
    public ClaUnsignedLong ReadClaUnsignedLong() => new(ReadUnsignedLongLE());

    /// <summary>
    /// Reads a <see cref="ClaSingleReal"/> and advances the current position.
    /// </summary>
    /// <returns></returns>
    public ClaSingleReal ReadClaFloat() => new(ReadFloatLE());

    /// <summary>
    /// Reads a <see cref="ClaReal"/> and advances the current position.
    /// </summary>
    /// <returns></returns>
    public ClaReal ReadClaDouble() => new(ReadDoubleLE());

    /// <summary>
    /// Reads a <see cref="ClaDecimal"/> and advances the current position.
    /// </summary>
    /// <param name="length">The total number of bytes that represent the number.</param>
    /// <param name="digitsAfterDecimalPoint">The number of digits in the fractional part of the number.</param>
    /// <returns></returns>
    public ClaDecimal ReadClaDecimal(int length, byte digitsAfterDecimalPoint)
    {
        if (length < 1 || length > 16)
        {
            throw new ArgumentOutOfRangeException(nameof(length), actualValue: length, "Expected a byte length between 1 and 16 inclusive.");
        }

        ulong high = default;
        ulong low = default;
        byte places = digitsAfterDecimalPoint;

        ref ulong current = ref length > 16 ? ref high : ref low;

        ReadOnlySpan<byte> data = ReadBytes(length).Span;

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

    /// <summary>
    /// Reads a <see cref="ClaFString"/> and consumes the entire data array.
    /// </summary>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public ClaFString ReadClaFString(Encoding? encoding = null)
    {
        encoding ??= Encoding;

        return new ClaFString(encoding.GetString(PeekBaseSpan()));
    }

    /// <summary>
    /// Reads a <see cref="ClaFString"/> and advances the current position.
    /// </summary>
    /// <param name="length">The length of the string in bytes.</param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public ClaFString ReadClaFString(int length, Encoding? encoding = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        encoding ??= Encoding;

        return new ClaFString(ReadFixedLengthString(length, encoding));
    }

    /// <summary>
    /// Reads a <see cref="ClaCString"/> and advances the current position.
    /// </summary>
    /// <returns></returns>
    public ClaCString ReadClaCString(Encoding? encoding = null) => new(ReadZeroTerminatedString(encoding));

    /// <summary>
    /// Reads a <see cref="ClaPString"/> and advances the current position.
    /// </summary>
    /// <returns></returns>
    public ClaPString ReadClaPString(Encoding? encoding = null) => new(ReadPascalString(encoding));
}
