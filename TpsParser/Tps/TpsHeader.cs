using System;
using System.Collections.Generic;
using System.Text;
using TpsParser.Binary;

namespace TpsParser.Tps
{
    public sealed class TpsHeader
    {
        public int Address { get; }
        public int HeaderSize { get; }
        public int FileLength1 { get; }
        public int FileLength2 { get; }
        private string TopSpeed { get; }
        public int Zeroes { get; }
        public int LastIssuedRow { get; }
        public int Changes { get; }
        public int ManagementPageReference { get; }

        public IList<int> PageStart { get; }
        public IList<int> PageEnd { get; }

        private RandomAccess Data { get; }

        public bool IsTopSpeedFile => TopSpeed == "tOpS";

        public TpsHeader(RandomAccess rx)
        {
            Data = rx ?? throw new ArgumentNullException(nameof(rx));

            Address = rx.LongLE();

            if (Address != 0)
            {
                throw new NotATopSpeedFileException("File does not start with 0x00000000. It is not a TopSpeed file or it may be encrypted.");
            }

            HeaderSize = rx.ShortLE();

            var header = rx.Read(HeaderSize - 6);

            FileLength1 = header.LongLE();
            FileLength2 = header.LongLE();
            TopSpeed = header.FixedLengthString(4);
            Zeroes = header.ShortLE();
            LastIssuedRow = header.LongBE();
            Changes = header.LongLE();
            ManagementPageReference = header.ToFileOffset(header.LongLE());

            PageStart = header.ToFileOffset(header.LongArrayLE((0x110 - 0x20) / 4));
            PageEnd = header.ToFileOffset(header.LongArrayLE((0x200 - 0x110) / 4));
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"TpsHeader({Data.ToHex8(Address)},{Data.ToHex4(HeaderSize)},{Data.ToHex8(FileLength1)},{Data.ToHex8(FileLength2)}," +
                $"{TopSpeed},{Data.ToHex4(Zeroes)},{Data.ToHex8(LastIssuedRow)},{Data.ToHex8(Changes)},{Data.ToHex8(ManagementPageReference)})");

            for (int i = 0; i < PageStart.Count; i++)
            {
                sb.AppendLine($"{PageStart[i]}..{PageEnd[i]}");
            }

            return sb.ToString();
        }
    }
}
