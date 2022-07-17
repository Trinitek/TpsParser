using System;
using System.Collections.Generic;
using System.Text;

namespace TpsParser.Tps
{
    /// <summary>
    /// Represents a TopSpeed file header.
    /// </summary>
    public sealed class FileHeader
    {
        /// <summary>
        /// Gets the magic number used to identify a TopSpeed file.
        /// </summary>
        public const string TopSpeedMagicNumber = "tOpS";

        /// <summary>
        /// Gets the size of the header in bytes.
        /// </summary>
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

        /// <summary>
        /// Returns true if the header represents a valid TopSpeed file.
        /// </summary>
        public bool IsTopSpeedFile => MagicNumber == TopSpeedMagicNumber;

        /// <summary>
        /// Instantiates a new file header.
        /// </summary>
        /// <param name="headerSize"></param>
        /// <param name="fileLength1"></param>
        /// <param name="fileLength2"></param>
        /// <param name="magicNumber"></param>
        /// <param name="zeroes"></param>
        /// <param name="lastIssuedRow"></param>
        /// <param name="changes"></param>
        /// <param name="managementPageReference"></param>
        /// <param name="pageStart"></param>
        /// <param name="pageEnd"></param>
        public FileHeader(
            short headerSize,
            int fileLength1,
            int fileLength2,
            string magicNumber,
            short zeroes,
            int lastIssuedRow,
            int changes,
            int managementPageReference,
            IReadOnlyList<int> pageStart,
            IReadOnlyList<int> pageEnd)
        {
            HeaderSize = headerSize;
            FileLength1 = fileLength1;
            FileLength2 = fileLength2;
            MagicNumber = magicNumber ?? throw new ArgumentNullException(nameof(magicNumber));
            Zeroes = zeroes;
            LastIssuedRow = lastIssuedRow;
            Changes = changes;
            ManagementPageReference = managementPageReference;
            PageStart = pageStart ?? throw new ArgumentNullException(nameof(pageStart));
            PageEnd = pageEnd ?? throw new ArgumentNullException(nameof(pageEnd));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"TpsHeader({StringUtils.ToHex4(HeaderSize)},{StringUtils.ToHex8(FileLength1)},{StringUtils.ToHex8(FileLength2)}," +
                $"{MagicNumber},{StringUtils.ToHex4(Zeroes)},{StringUtils.ToHex8(LastIssuedRow)},{StringUtils.ToHex8(Changes)},{StringUtils.ToHex8(ManagementPageReference)})");

            for (int i = 0; i < PageStart.Count; i++)
            {
                sb.AppendLine($"{PageStart[i]}..{PageEnd[i]}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates a new <see cref="FileHeader"/> from the given reader.
        /// </summary>
        /// <param name="rx"></param>
        /// <returns></returns>
        public static FileHeader Read(TpsReader rx)
        {
            if (rx is null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            int address = rx.ReadLongLE();

            if (address != 0)
            {
                throw new NotATopSpeedFileException("File does not start with 0x00000000. It is not a TopSpeed file or it may be encrypted.");
            }

            short headerSize = rx.ReadShortLE();

            var headerReader = rx.Read(headerSize - 6);

            return new FileHeader(
                headerSize: headerSize,
                fileLength1: headerReader.ReadLongLE(),
                fileLength2: headerReader.ReadLongLE(),
                magicNumber: headerReader.ReadFixedLengthString(4),
                zeroes: headerReader.ReadShortLE(),
                lastIssuedRow: headerReader.ReadLongBE(),
                changes: headerReader.ReadLongLE(),
                managementPageReference: TpsReader.GetFileOffset(headerReader.ReadLongLE()),
                pageStart: TpsReader.GetFileOffset(headerReader.LongArrayLE((0x110 - 0x20) / 4)),
                pageEnd: TpsReader.GetFileOffset(headerReader.LongArrayLE((0x200 - 0x110) / 4)));
        }
    }
}
