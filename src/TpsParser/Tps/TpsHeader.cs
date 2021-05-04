using System;
using System.Collections.Generic;
using System.Text;

namespace TpsParser.Tps
{
    /// <summary>
    /// Represents a TopSpeed file header.
    /// </summary>
    public sealed class TpsHeader
    {
        public int Address { get; }
        public int HeaderSize { get; }
        public int FileLength1 { get; }
        public int FileLength2 { get; }

        /// <summary>
        /// Gets the magic number signature in the TopSpeed file header. This should be 'tOpS' for all TPS files.
        /// </summary>
        public string MagicNumber { get; }

        public int Zeroes { get; }

        /// <summary>
        /// Gets the last issued row number in the file.
        /// </summary>
        public int LastIssuedRow { get; }

        public int Changes { get; }

        public int ManagementPageReference { get; }

        public IReadOnlyList<int> PageStart { get; }

        public IReadOnlyList<int> PageEnd { get; }

        private TpsReader Data { get; }

        /// <summary>
        /// Returns true if the header represents a valid TopSpeed file.
        /// </summary>
        public bool IsTopSpeedFile => MagicNumber == "tOpS";

        public TpsHeader(TpsReader rx)
        {
            Data = rx ?? throw new ArgumentNullException(nameof(rx));

            Address = rx.ReadLongLE();

            if (Address != 0)
            {
                throw new NotATopSpeedFileException("File does not start with 0x00000000. It is not a TopSpeed file or it may be encrypted.");
            }

            HeaderSize = rx.ReadShortLE();

            var header = rx.Read(HeaderSize - 6);

            FileLength1 = header.ReadLongLE();
            FileLength2 = header.ReadLongLE();
            MagicNumber = header.FixedLengthString(4);
            Zeroes = header.ReadShortLE();
            LastIssuedRow = header.ReadLongBE();
            Changes = header.ReadLongLE();
            ManagementPageReference = header.ToFileOffset(header.ReadLongLE());

            PageStart = header.ToFileOffset(header.LongArrayLE((0x110 - 0x20) / 4));
            PageEnd = header.ToFileOffset(header.LongArrayLE((0x200 - 0x110) / 4));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"TpsHeader({StringUtils.ToHex8(Address)},{StringUtils.ToHex4(HeaderSize)},{StringUtils.ToHex8(FileLength1)},{StringUtils.ToHex8(FileLength2)}," +
                $"{MagicNumber},{StringUtils.ToHex4(Zeroes)},{StringUtils.ToHex8(LastIssuedRow)},{StringUtils.ToHex8(Changes)},{StringUtils.ToHex8(ManagementPageReference)})");

            for (int i = 0; i < PageStart.Count; i++)
            {
                sb.AppendLine($"{PageStart[i]}..{PageEnd[i]}");
            }

            return sb.ToString();
        }
    }
}
