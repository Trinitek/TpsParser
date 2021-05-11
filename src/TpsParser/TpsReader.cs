using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TpsParser.Tps.Type;

namespace TpsParser
{
    public sealed class TpsReader
    {
        private byte[] Data { get; }
        private Stack<int> PositionStack { get; }

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
        public TpsReader(byte[] data)
            : this(
                  data: data,
                  baseOffset: 0,
                  length: data?.Length ?? throw new ArgumentNullException(nameof(data)))
        { }

        /// <summary>
        /// Instantiates a new reader from a byte array.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="baseOffset"></param>
        /// <param name="length"></param>
        public TpsReader(byte[] data, int baseOffset, int length)
        {
            Position = 0;
            Data = data ?? throw new ArgumentNullException(nameof(data));
            BaseOffset = baseOffset;
            Length = length;

            PositionStack = new Stack<int>();
        }

        /// <summary>
        /// Instantiates a new reader from an existing one.
        /// </summary>
        /// <param name="existing"></param>
        /// <param name="additiveOffset"></param>
        /// <param name="length"></param>
        public TpsReader(TpsReader existing, int additiveOffset, int length)
            : this(
                  data: existing?.Data ?? throw new ArgumentNullException(nameof(existing)),
                  baseOffset: existing.BaseOffset + additiveOffset,
                  length: length)
        { }

        /// <summary>
        /// Saves the current position on the stack.
        /// </summary>
        public void PushPosition() => PositionStack.Push(Position);

        /// <summary>
        /// Restores the previous position saved to the stack with <see cref="PushPosition"/>.
        /// </summary>
        public void PopPosition() => Position = PositionStack.Pop();

        private void AssertSpace(int numberOfBytes)
        {
            if (Position + numberOfBytes > Length)
            {
                throw new IndexOutOfRangeException($"Data type of size {numberOfBytes} exceeds the end of the data array at offset {Position} by {Length - Position + numberOfBytes}. Array is {Length} bytes long.");
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
        public int ReadLongLE()
        {
            AssertSpace(4);

            int result =
                Data[AbsolutePosition + 0] & 0xFF
                | (Data[AbsolutePosition + 1] & 0xFF) << 8
                | (Data[AbsolutePosition + 2] & 0xFF) << 16
                | (Data[AbsolutePosition + 3] & 0xFF) << 24;

            Position += 4;
            return result;
        }

        /// <summary>
        /// Writes a 4 byte little endian integer to the current position. This is typically used when decrypting.
        /// </summary>
        /// <param name="value"></param>
        public void WriteLongLE(int value)
        {
            AssertSpace(4);

            Data[AbsolutePosition + 0] = (byte)(value & 0xFF);
            Data[AbsolutePosition + 1] = (byte)(value >> 8 & 0xFF);
            Data[AbsolutePosition + 2] = (byte)(value >> 16 & 0xFF);
            Data[AbsolutePosition + 3] = (byte)(value >> 24 & 0xFF);

            Position += 4;
        }

        /// <summary>
        /// Reads a little endian unsigned 4 byte integer.
        /// </summary>
        /// <returns></returns>
        public uint ReadUnsignedLongLE()
        {
            AssertSpace(4);

            uint result =
                Data[AbsolutePosition + 0] & 0xFFU
                | (Data[AbsolutePosition + 1] & 0xFFU) << 8
                | (Data[AbsolutePosition + 2] & 0xFFU) << 16
                | (Data[AbsolutePosition + 3] & 0xFFU) << 24;

            Position += 4;
            return result;
        }

        /// <summary>
        /// Reads a big endian signed integer and advances the current position.
        /// </summary>
        /// <returns></returns>
        public int ReadLongBE()
        {
            AssertSpace(4);

            int reference = BaseOffset + Position;

            int result =
                Data[AbsolutePosition + 3] & 0xFF
                | (Data[AbsolutePosition + 2] & 0xFF) << 8
                | (Data[AbsolutePosition + 1] & 0xFF) << 16
                | (Data[AbsolutePosition + 0] & 0xFF) << 24;

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

            uint result =
                Data[AbsolutePosition + 4] & 0xFFU
                | (Data[AbsolutePosition + 3] & 0xFFU) << 8
                | (Data[AbsolutePosition + 2] & 0xFFU) << 16
                | (Data[AbsolutePosition + 1] & 0xFFU) << 24;

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

            short result =
                (short)(Data[AbsolutePosition + 0] & 0xFF
                | (Data[AbsolutePosition + 1] & 0xFF) << 8);

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

            ushort result =
                (ushort)(Data[AbsolutePosition + 0] & 0xFF
                | (Data[AbsolutePosition + 1] & 0xFF) << 8);

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

            short result =
                (short)(Data[AbsolutePosition + 1] & 0xFF
                | (Data[AbsolutePosition + 0] & 0xFF) << 8);

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
        /// Reads a byte from the given position without advancing the position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public byte Peek(int position) => Data[BaseOffset + position];

        /// <summary>
        /// Reads a little endian float and advances the current position.
        /// </summary>
        /// <returns></returns>
        public float ReadFloatLE()
        {
            int integer = ReadLongLE();
            byte[] intBytes = BitConverter.GetBytes(integer);
            float result = BitConverter.ToSingle(intBytes, 0);
            return result;
        }

        /// <summary>
        /// Reads a little endian double and advances the current position.
        /// </summary>
        /// <returns></returns>
        public double ReadDoubleLE()
        {
            long lsb = ReadLongLE() & 0xFFFFFFFFL;
            long msb = ReadLongLE() & 0xFFFFFFFFL;

            long doubleAsLong = msb << 32 | lsb;

            double result = BitConverter.Int64BitsToDouble(doubleAsLong);

            return result;
        }

        /// <summary>
        /// Reads a fixed length string and advances the current position.
        /// </summary>
        /// <param name="length">The length of the string to read.</param>
        /// <returns></returns>
        public string FixedLengthString(int length) => ReadFixedLengthString(length, TpsParser.DefaultEncoding);

        /// <summary>
        /// Reads a fixed length string and advances the current position.
        /// </summary>
        /// <param name="length">The length of the string to read.</param>
        /// <param name="encoding">The encoding of the string.</param>
        /// <returns></returns>
        public string ReadFixedLengthString(int length, Encoding encoding)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            AssertSpace(length);

            string result = encoding.GetString(Data.ToArray(), AbsolutePosition, length);

            Position += length;

            return result;
        }

        /// <summary>
        /// Reads a zero-terminated string and advances the current position.
        /// </summary>
        /// <returns></returns>
        public string ZeroTerminatedString() => ReadZeroTerminatedString(TpsParser.DefaultEncoding);

        /// <summary>
        /// Reads a zero-terminated string and advances the current position.
        /// </summary>
        /// <param name="encoding">The encoding of the string.</param>
        /// <returns></returns>
        public string ReadZeroTerminatedString(Encoding encoding)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

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

            return encoding.GetString(bytes.ToArray());
        }

        /// <summary>
        /// Reads a Pascal string and advances the current position. Pascal strings have their length encoded in the first byte.
        /// </summary>
        /// <param name="encoding">The encoding of the string.</param>
        /// <returns></returns>
        public string ReadPascalString(Encoding encoding)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            int length = ReadByte();

            var bytes = new List<byte>();

            for (int i = 0; i < length; i++)
            {
                bytes.Add(ReadByte());
            }

            return encoding.GetString(bytes.ToArray());
        }

        /// <summary>
        /// Sets the current position to the given offset.
        /// </summary>
        /// <param name="offset">The new offset.</param>
        /// <returns></returns>
        public TpsReader JumpAbsolute(int offset)
        {
            Position = offset;
            return this;
        }

        /// <summary>
        /// Sets the current position relative to the given offset.
        /// </summary>
        /// <param name="offset">The relative offset.</param>
        /// <returns></returns>
        public TpsReader JumpRelative(int offset)
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
        /// Gets a new <see cref="TpsReader"/> of the given length at the current position and advances the position.
        /// </summary>
        /// <param name="length">The length of the data array.</param>
        /// <returns></returns>
        public TpsReader Read(int length)
        {
            AssertSpace(length);

            int reference = AbsolutePosition;
            Position += length;

            return new TpsReader(Data, reference, length);
        }

        /// <summary>
        /// Reads an array of bytes and advances the position.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] ReadBytes(int length)
        {
            var dest = PeekBytes(length);

            Position += length;

            return dest;
        }

        /// <summary>
        /// Reads an array from the current position.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] PeekBytes(int length)
        {
            AssertSpace(length);

            byte[] dest = new byte[length];

            Array.Copy(Data, AbsolutePosition, dest, 0, length);

            return dest;
        }

        /// <summary>
        /// Unpacks a run length encoded sequence of bytes.
        /// </summary>
        /// <returns></returns>
        public TpsReader UnpackRunLengthEncoding()
        {
            var bytes = new List<byte>();

            do
            {
                int skip = ReadByte();

                if (skip == 0)
                {
                    throw new RunLengthEncodingException("Bad RLE Skip (0x00)");
                }

                if (skip > 0x7F)
                {
                    int msb = ReadByte();
                    int lsb = skip & 0x7F;
                    int shift = 0x80 * (msb & 0x01);
                    skip = (msb << 7 & 0xFF00) + lsb + shift;
                }

                bytes.AddRange(ReadBytes(skip));

                if (!IsOneByteLeft)
                {
                    JumpRelative(-1);

                    byte toRepeat = ReadByte();
                    int repeatsMinusOne = ReadByte();

                    if (repeatsMinusOne > 0x7F)
                    {
                        int msb = ReadByte();
                        int lsb = repeatsMinusOne & 0x7F;
                        int shift = 0x80 * (msb & 0x01);
                        repeatsMinusOne = (msb << 7 & 0xFF00) + lsb + shift;
                    }

                    byte[] repeat = new byte[repeatsMinusOne];

                    for (int i = 0; i < repeatsMinusOne; i++)
                    {
                        repeat[i] = toRepeat;
                    }

                    bytes.AddRange(repeat);
                }
            }
            while (!IsAtEnd);

            return new TpsReader(bytes.ToArray());
        }

        /// <summary>
        /// Reads an array of little endian 2s-complement signed 4 byte integers.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public int[] LongArrayLE(int length)
        {
            int[] results = new int[length];

            for (int i = 0; i < length; i++)
            {
                results[i] = ReadLongLE();
            }

            return results;
        }

        /// <summary>
        /// Gets an array of the remaining unread data array.
        /// </summary>
        /// <returns></returns>
        public byte[] GetRemainder()
        {
            byte[] result = new byte[Length - Position];

            Array.Copy(
                sourceArray: Data,
                sourceIndex: AbsolutePosition,
                destinationArray: result,
                destinationIndex: 0,
                length: result.Length);

            return result;
        }

        public int ToFileOffset(int pageReference) => (pageReference << 8) + 0x200;

        public int[] ToFileOffset(int[] pageReferences)
        {
            return pageReferences
                .Select(reference => ToFileOffset(reference))
                .ToArray();
        }

        public string ToAscii()
        {
            var stringBuilder = new StringBuilder();

            bool wasHex = false;

            for (int i = 0; i < Length; i++)
            {
                int value = Data[BaseOffset + i];

                if (value < 32 | value > 127)
                {
                    stringBuilder.Append(" ");
                    stringBuilder.Append(StringUtils.ToHex2(value));
                    wasHex = true;
                }
                else
                {
                    if (wasHex)
                    {
                        stringBuilder.Append(" ");
                        wasHex = false;
                    }
                    if (value == 32)
                    {
                        stringBuilder.Append(".");
                    }
                    else
                    {
                        stringBuilder.Append((char)value);
                    }
                }
            }

            return stringBuilder.ToString();
        }

        public string ReadBinaryCodedDecimal(int length, int digitsAfterDecimalPoint)
        {
            var stringBuilder = new StringBuilder();

            foreach (byte b in ReadBytes(length))
            {
                stringBuilder.Append(StringUtils.ToHex2(b));
            }

            string currentString = stringBuilder.ToString();

            string sign = currentString.Substring(0, 1);
            string number = currentString.Substring(1);

            if (digitsAfterDecimalPoint > 0)
            {
                int decimalIndex = number.Length - digitsAfterDecimalPoint;
                number = TrimLeadingZeroes(number.Substring(0, decimalIndex)) + "." + number.Substring(decimalIndex);
            }
            else
            {
                number = TrimLeadingZeroes(number);
            }

            return (!(sign == "0") ? "-" : string.Empty) + number;
        }

        private string TrimLeadingZeroes(string number) => string.IsNullOrWhiteSpace(number) ? "0" : decimal.Parse(number).ToString();

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Position:X}/{Length:X}";
        }

        public string ToHexString(int step, bool ascii)
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
                    sb.Append(" ");

                    for (int y = 0; y < step; y++)
                    {
                        if (i + y < Length)
                        {
                            int ch = Data[BaseOffset + i + y] & 0xFF;

                            if (ch < 32 && ch > 127)
                            {
                                sb.Append(".");
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
        /// Reads a <see cref="TpsByte"/> and advances the current position.
        /// </summary>
        /// <returns></returns>
        public TpsByte ReadTpsByte() => new TpsByte(ReadByte());

        /// <summary>
        /// Reads a <see cref="TpsShort"/> and advances the current position.
        /// </summary>
        /// <returns></returns>
        public TpsShort ReadTpsShort() => new TpsShort(ReadShortLE());

        /// <summary>
        /// Reads a <see cref="TpsUnsignedShort"/> and advances the current position.
        /// </summary>
        /// <returns></returns>
        public TpsUnsignedShort ReadTpsUnsignedShort() => new TpsUnsignedShort(ReadUnsignedShortLE());

        /// <summary>
        /// Reads a <see cref="TpsDate"/> and advances the current position.
        /// </summary>
        /// <returns></returns>
        public TpsDate ReadTpsDate()
        {
            long date = ReadUnsignedLongLE();

            if (date != 0)
            {
                long years = (date & 0xFFFF0000) >> 16;
                long months = (date & 0x0000FF00) >> 8;
                long days = date & 0x000000FF;
                return new TpsDate(new DateTime((int)years, (int)months, (int)days));
            }
            else
            {
                return new TpsDate(null);
            }
        }

        /// <summary>
        /// Reads a <see cref="TpsTime"/> and advances the current position.
        /// </summary>
        /// <returns></returns>
        public TpsTime ReadTpsTime()
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

            return new TpsTime(new TimeSpan(0, hours, mins, secs, centi * 10));
        }

        /// <summary>
        /// Reads a <see cref="TpsLong"/> and advances the current position.
        /// </summary>
        /// <returns></returns>
        public TpsLong ReadTpsLong() => new TpsLong(ReadLongLE());

        /// <summary>
        /// Reads a <see cref="TpsUnsignedLong"/> and advances the current position.
        /// </summary>
        /// <returns></returns>
        public TpsUnsignedLong ReadTpsUnsignedLong() => new TpsUnsignedLong(ReadUnsignedLongLE());

        /// <summary>
        /// Reads a <see cref="TpsFloat"/> and advances the current position.
        /// </summary>
        /// <returns></returns>
        public TpsFloat ReadTpsFloat() => new TpsFloat(ReadFloatLE());

        /// <summary>
        /// Reads a <see cref="TpsDouble"/> and advances the current position.
        /// </summary>
        /// <returns></returns>
        public TpsDouble ReadTpsDouble() => new TpsDouble(ReadDoubleLE());

        /// <summary>
        /// Reads a <see cref="TpsDecimal"/> and advances the current position.
        /// </summary>
        /// <param name="length">The total number of bytes that represent the number.</param>
        /// <param name="digitsAfterDecimalPoint">The number of digits in the fractional part of the number.</param>
        /// <returns></returns>
        public TpsDecimal ReadTpsDecimal(int length, byte digitsAfterDecimalPoint)
        {
            if (length < 1 || length > 16)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Expected a byte length between 1 and 16 inclusive.");
            }

            ulong high = default;
            ulong low = default;
            byte places = digitsAfterDecimalPoint;

            ref ulong current = ref (length > 16) ? ref high : ref low;

            byte[] data = ReadBytes(length);

            int shift = 0;

            // Write the least significant 30 digits
            for (int i = length - 1; i > 0; i--)
            {
                current |= (ulong)data[i] << (8 * shift);

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
            current |= ((ulong)data[0] & 0x0F) << (8 * shift);

            // Sign
            high |= ((ulong)data[0] & 0xF0) << 56;

            return new TpsDecimal(high, low, places);
        }

        /// <summary>
        /// Reads a <see cref="TpsString"/> and advances the current position.
        /// </summary>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public TpsString ReadTpsString(Encoding encoding)
        {
            if (encoding is null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            return new TpsString(encoding.GetString(GetData()));
        }

        /// <summary>
        /// Reads a <see cref="TpsString"/> and advances the current position.
        /// </summary>
        /// <param name="encoding"></param>
        /// <param name="length">The length of the string in bytes.</param>
        /// <returns></returns>
        public TpsString ReadTpsString(Encoding encoding, int length)
        {
            if (encoding is null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "String length must not be negative.");
            }

            return new TpsString(ReadFixedLengthString(length, encoding));
        }

        /// <summary>
        /// Reads a <see cref="TpsCString"/> and advances the current position.
        /// </summary>
        /// <returns></returns>
        public TpsCString ReadTpsCString(Encoding encoding) => new TpsCString(ReadZeroTerminatedString(encoding));

        /// <summary>
        /// Reads a <see cref="TpsPString"/> and advances the current position.
        /// </summary>
        /// <returns></returns>
        public TpsPString ReadTpsPString(Encoding encoding) => new TpsPString(ReadPascalString(encoding));
    }
}
