﻿using System;
using System.Collections.Generic;
using System.Text;
using TpsParser.Binary;

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

            Address = rx.LongLE();

            if (Address != 0)
            {
                throw new NotATopSpeedFileException("File does not start with 0x00000000. It is not a TopSpeed file or it may be encrypted.");
            }

            HeaderSize = rx.ShortLE();

            var header = rx.Read(HeaderSize - 6);

            FileLength1 = header.LongLE();
            FileLength2 = header.LongLE();
            MagicNumber = header.FixedLengthString(4);
            Zeroes = header.ShortLE();
            LastIssuedRow = header.LongBE();
            Changes = header.LongLE();
            ManagementPageReference = header.ToFileOffset(header.LongLE());

            PageStart = header.ToFileOffset(header.LongArrayLE((0x110 - 0x20) / 4));
            PageEnd = header.ToFileOffset(header.LongArrayLE((0x200 - 0x110) / 4));
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override string ToString()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            var sb = new StringBuilder();

            sb.AppendLine($"TpsHeader({Data.ToHex8(Address)},{Data.ToHex4(HeaderSize)},{Data.ToHex8(FileLength1)},{Data.ToHex8(FileLength2)}," +
                $"{MagicNumber},{Data.ToHex4(Zeroes)},{Data.ToHex8(LastIssuedRow)},{Data.ToHex8(Changes)},{Data.ToHex8(ManagementPageReference)})");

            for (int i = 0; i < PageStart.Count; i++)
            {
                sb.AppendLine($"{PageStart[i]}..{PageEnd[i]}");
            }

            return sb.ToString();
        }
    }
}
