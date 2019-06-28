using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TpsParser.Binary
{
    public sealed class RandomAccess
    {
        private byte[] Data { get; }
        private Stack<int> PositionStack { get; }
        private int BaseOffset { get; }

        /// <summary>
        /// Gets the current position in the data array.
        /// </summary>
        public int Position { get; private set; }

        /// <summary>
        /// Gets the length of the data array.
        /// </summary>
        public int Length { get; }

        public bool IsOneByteLeft => Position > Length - 1;

        public bool IsAtEnd => Position >= Length - 1;

        public RandomAccess(byte[] data)
            : this(
                  data: data,
                  baseOffset: 0,
                  length: data.Length)
        { }

        public RandomAccess(byte[] data, int baseOffset, int length)
        {
            Position = 0;
            Data = data ?? throw new ArgumentNullException(nameof(data));
            BaseOffset = baseOffset;
            Length = length;

            PositionStack = new Stack<int>();
        }

        public void PushPosition() => PositionStack.Push(Position);

        public void PopPosition() => Position = PositionStack.Pop();

        private void CheckSpace(int numberOfBytes)
        {
            if (Position + numberOfBytes > Length)
            {
                throw new IndexOutOfRangeException($"Data type of size {numberOfBytes} exceeds the end of the data array at offset {Position}. Array is {Length} bytes long.");
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
        public int LongLE()
        {
            CheckSpace(4);

            int reference = BaseOffset + Position;

            int result =
                (Data[reference + 0] & 0xFF)
                | ((Data[reference + 1] & 0xFF) << 8)
                | ((Data[reference + 2] & 0xFF) << 16)
                | ((Data[reference + 3] & 0xFF) << 24);

            Position += 4;
            return result;
        }

        /// <summary>
        /// Writes a 4 byte little endian integer to the current position. This is typically used when decrypting.
        /// </summary>
        /// <param name="value"></param>
        public void WriteLongLE(int value)
        {
            CheckSpace(4);

            int reference = BaseOffset + Position;

            Data[reference + 0] = (byte)(value & 0xFF);
            Data[reference + 1] = (byte)((value >> 8) & 0xFF);
            Data[reference + 2] = (byte)((value >> 16) & 0xFF);
            Data[reference + 3] = (byte)((value >> 24) & 0xFF);

            Position += 4;
        }

        /// <summary>
        /// Reads a little endian unsigned 4 byte integer.
        /// </summary>
        /// <returns></returns>
        public uint UnsignedLongLE()
        {
            CheckSpace(4);

            int reference = BaseOffset + Position;

            uint result =
                (Data[reference + 0] & 0xFFU)
                | ((Data[reference + 1] & 0xFFU) << 8)
                | ((Data[reference + 2] & 0xFFU) << 16)
                | ((Data[reference + 3] & 0xFFU) << 24);

            Position += 4;
            return result;
        }

        /// <summary>
        /// Reads a big endian signed integer.
        /// </summary>
        /// <returns></returns>
        public int LongBE()
        {
            CheckSpace(4);

            int reference = BaseOffset + Position;

            int result =
                (Data[reference + 3] & 0xFF)
                | ((Data[reference + 2] & 0xFF) << 8)
                | ((Data[reference + 1] & 0xFF) << 16)
                | ((Data[reference + 0] & 0xFF) << 24);

            Position += 4;
            return result;
        }

        /// <summary>
        /// Reads a big endian unsigned integer.
        /// </summary>
        /// <returns></returns>
        public uint UnsignedLongBE()
        {
            CheckSpace(4);

            int reference = BaseOffset + Position;

            uint result =
                (Data[reference + 4] & 0xFFU)
                | ((Data[reference + 3] & 0xFFU) << 8)
                | ((Data[reference + 2] & 0xFFU) << 16)
                | ((Data[reference + 1] & 0xFFU) << 24);

            Position += 4;
            return result;
        }

        /// <summary>
        /// Reads a little endian signed short.
        /// </summary>
        /// <returns></returns>
        public short ShortLE()
        {
            CheckSpace(2);

            int reference = BaseOffset + Position;

            short result =
                (short)((Data[reference + 0] & 0xFF)
                | ((Data[reference + 1] & 0xFF) << 8));

            Position += 2;

            return result;
        }

        /// <summary>
        /// Reads a little endian unsigned short.
        /// </summary>
        /// <returns></returns>
        public ushort UnsignedShortLE()
        {
            CheckSpace(2);

            int reference = BaseOffset + Position;

            ushort result =
                (ushort)((Data[reference + 0] & 0xFF)
                | ((Data[reference + 1] & 0xFF) << 8));

            Position += 2;

            return result;
        }

        /// <summary>
        /// Reads a big endian signed short.
        /// </summary>
        /// <returns></returns>
        public short ShortBE()
        {
            CheckSpace(2);

            int reference = BaseOffset + Position;

            short result =
                (short)((Data[reference + 1] & 0xFF)
                | ((Data[reference + 0] & 0xFF) << 8));

            Position += 2;

            return result;
        }

        /// <summary>
        /// Reads a byte.
        /// </summary>
        /// <returns></returns>
        public byte Byte()
        {
            CheckSpace(1);

            int reference = BaseOffset + Position;

            byte result = Data[reference];

            Position += 1;

            return result;
        }

        public byte Peek(int position) => Data[BaseOffset + position];

        /// <summary>
        /// Reads a little endian float.
        /// </summary>
        /// <returns></returns>
        public float FloatLE()
        {
            int integer = LongLE();
            byte[] intBytes = BitConverter.GetBytes(integer);
            float result = BitConverter.ToSingle(intBytes, 0);
            return result;
        }

        /// <summary>
        /// Reads a little endian double.
        /// </summary>
        /// <returns></returns>
        public double DoubleLE()
        {
            long lsb = LongLE() & 0xFFFFFFFFL;
            long msb = LongLE() & 0xFFFFFFFFL;

            long doubleAsLong = (msb << 32) | lsb;

            double result = BitConverter.Int64BitsToDouble(doubleAsLong);

            return result;
        }

        /// <summary>
        /// Reads a fixed length string.
        /// </summary>
        /// <param name="length">The length of the string to read.</param>
        /// <returns></returns>
        public string FixedLengthString(int length) => FixedLengthString(length, Encoding.GetEncoding("ISO-8859-1"));

        /// <summary>
        /// Reads a fixed length string.
        /// </summary>
        /// <param name="length">The length of the string to read.</param>
        /// <param name="encoding">The encoding of the string.</param>
        /// <returns></returns>
        public string FixedLengthString(int length, Encoding encoding)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            CheckSpace(length);

            int reference = BaseOffset + Position;
            string result = encoding.GetString(Data.ToArray(), reference, length);

            Position += length;

            return result;
        }

        /// <summary>
        /// Reads a zero-terminated string.
        /// </summary>
        /// <returns></returns>
        public string ZeroTerminatedString() => ZeroTerminatedString(Encoding.GetEncoding("ISO-8859-1"));

        /// <summary>
        /// Reads a zero-terminated string.
        /// </summary>
        /// <param name="encoding">The encoding of the string.</param>
        /// <returns></returns>
        public string ZeroTerminatedString(Encoding encoding)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            var bytes = new List<byte>();

            byte value;

            do
            {
                value = Byte();

                if (value != 0)
                {
                    bytes.Add(value);
                }
            }
            while (value != 0);

            return encoding.GetString(bytes.ToArray());
        }

        /// <summary>
        /// Reads a Pascal string. Pascal strings have their length encoded in the first byte.
        /// </summary>
        /// <returns></returns>
        public string PascalString() => PascalString(Encoding.GetEncoding("ISO-8859-1"));

        /// <summary>
        /// Reads a Pascal string. Pascal strings have their length encoded in the first byte.
        /// </summary>
        /// <param name="encoding">The encoding of the string.</param>
        /// <returns></returns>
        public string PascalString(Encoding encoding)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            int length = Byte();

            var bytes = new List<byte>();

            for (int i = 0; i < length; i++)
            {
                bytes.Add(Byte());
            }

            return encoding.GetString(bytes.ToArray());
        }

        /// <summary>
        /// Set the current position to the given offset.
        /// </summary>
        /// <param name="offset">The new offset.</param>
        /// <returns></returns>
        public RandomAccess JumpAbsolute(int offset)
        {
            Position = offset;
            return this;
        }

        /// <summary>
        /// Set the current position relative to the given offset.
        /// </summary>
        /// <param name="offset">The relative offset.</param>
        /// <returns></returns>
        public RandomAccess JumpRelative(int offset)
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
                return Data
                    .Skip(BaseOffset)
                    .Take(Length)
                    .ToArray();
            }
        }

        /// <summary>
        /// Gets a new <see cref="RandomAccess"/> of the given length at the current position and advances the position.
        /// </summary>
        /// <param name="length">The length of the data array.</param>
        /// <returns></returns>
        public RandomAccess Read(int length)
        {
            CheckSpace(length);

            int reference = BaseOffset + Position;
            Position += length;

            return new RandomAccess(Data, reference, length);
        }

        /// <summary>
        /// Reads an array from the current position and advances the position.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] ReadBytes(int length)
        {
            CheckSpace(length);

            int reference = BaseOffset + Position;

            Position += length;

            return Data
                .Skip(reference)
                .Take(length)
                .ToArray();
        }

        /// <summary>
        /// Unpacks a run length encoded sequence of bytes.
        /// </summary>
        /// <returns></returns>
        public RandomAccess UnpackRunLengthEncoding()
        {
            var bytes = new List<byte>();

            do
            {
                int skip = Byte();

                if (skip == 0)
                {
                    throw new RunLengthEncodingException("Bad RLE Skip (0x00)");
                }

                if (skip > 0x7F)
                {
                    int msb = Byte();
                    int lsb = skip & 0x7F;
                    int shift = 0x80 * (msb & 0x01);
                    skip = ((msb << 7) & 0xFF00) + lsb + shift;
                }

                bytes.AddRange(ReadBytes(skip));

                if (!IsOneByteLeft)
                {
                    JumpRelative(-1);

                    byte toRepeat = Byte();
                    int repeatsMinusOne = Byte();

                    if (repeatsMinusOne > 0x7F)
                    {
                        int msb = Byte();
                        int lsb = repeatsMinusOne & 0x7F;
                        int shift = 0x80 * (msb & 0x01);
                        repeatsMinusOne = ((msb << 7) & 0xFF00) + lsb + shift;
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

            return new RandomAccess(bytes.ToArray());
        }

        public string ToHex8(int value) => $"{value:X8}";

        public string ToHex4(int value) => $"{value:X4}";

        public string ToHex2(int value) => $"{value:X2}";

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
                results[i] = LongLE();
            }

            return results;
        }

        /// <summary>
        /// Gets an array of the remaining unread data array.
        /// </summary>
        /// <returns></returns>
        public byte[] GetRemainder()
        {
            int reference = BaseOffset + Position;

            byte[] result = new byte[Length - Position];

            Array.Copy(
                sourceArray: Data,
                sourceIndex: reference,
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

                if ((value < 32) | (value > 127))
                {
                    stringBuilder.Append(" ");
                    stringBuilder.Append(ToHex2(value));
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

        public string BinaryCodedDecimal(int length, int digitsAfterDecimalPoint)
        {
            var stringBuilder = new StringBuilder();

            foreach (byte b in ReadBytes(length))
            {
                stringBuilder.Append(ToHex2(b));
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

        public override string ToString()
        {
            return $"{Position:X}/{Length:X}";
        }
    }
}
