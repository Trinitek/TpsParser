using System;
using System.Collections.Generic;

namespace TpsParser.Tps
{
    /// <summary>
    /// Represents a page of <see cref="TpsRecord"/> objects.
    /// </summary>
    public sealed class Page
    {
        /// <summary>
        /// Gets the position of the page in the file.
        /// </summary>
        public int Address { get; }

        /// <summary>
        /// Gets the compressed size of the page in bytes.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Gets the uncompressed size of the page in bytes.
        /// </summary>
        public int SizeUncompressed { get; }

        /// <summary>
        /// Gets the uncompressed size of the page without the header in bytes.
        /// </summary>
        public int SizeUncompressedWithoutHeader { get; }

        /// <summary>
        /// Gets the number of records in the page.
        /// </summary>
        public int RecordCount { get; }

        /// <summary>
        /// Gets a set of undocumented bit flags.
        /// </summary>
        public byte Flags { get; }

        private TpsReader CompressedData { get; }
        private List<TpsRecord> Records { get; }

        private TpsReader _data;

        private bool IsFlushed => _data is null;

        public Page(TpsReader rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Records = new List<TpsRecord>();

            Address = rx.ReadLongLE();
            Size = rx.ReadShortLE();

            var header = rx.Read(Size - 6);

            SizeUncompressed = header.ReadShortLE();
            SizeUncompressedWithoutHeader = header.ReadShortLE();
            RecordCount = header.ReadShortLE();
            Flags = header.ReadByte();

            CompressedData = header.Read(Size - 13);
        }

        private void Decompress()
        {
            if ((Size != SizeUncompressed)
                && (Flags == 0))
            {
                try
                {
                    CompressedData.PushPosition();
                    _data = CompressedData.UnpackRunLengthEncoding();
                }
                catch (Exception ex)
                {
                    throw new RunLengthEncodingException($"Bad RLE data block at index {CompressedData} in {ToString()}", ex);
                }
                finally
                {
                    CompressedData.PopPosition();
                }
            }
            else
            {
                _data = CompressedData;
            }
        }

        public void Flush()
        {
            _data = null;
            Records.Clear();
        }

        public TpsReader GetUncompressedData()
        {
            if (IsFlushed)
            {
                Decompress();
            }

            return _data;
        }

        public void ParseRecords()
        {
            var rx = GetUncompressedData();

            Records.Clear();

            // Skip pages with non 0x00 flags as they don't seem to contain TpsRecords.
            if (Flags == 0x00)
            {
                rx.PushPosition();

                try
                {
                    TpsRecord previousRecord = null;

                    do
                    {
                        TpsRecord currentRecord
                            = previousRecord is null
                            ? new TpsRecord(rx)
                            : new TpsRecord(previousRecord, rx);

                        Records.Add(currentRecord);

                        previousRecord = currentRecord;
                    }
                    while (!rx.IsAtEnd && Records.Count < RecordCount);
                }
                finally
                {
                    rx.PopPosition();
                }
            }
        } 

        public IEnumerable<TpsRecord> GetRecords()
        {
            if (IsFlushed)
            {
                ParseRecords();
            }

            return Records;
        }

        /// <inheritdoc/>
        public override string ToString() =>
            $"TpsPage({Address:X8},{Size:X4},{SizeUncompressed:X4},{SizeUncompressedWithoutHeader:X4},{RecordCount:X4},{Flags:X2})";
    }
}
