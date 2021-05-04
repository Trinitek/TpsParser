using System;
using System.Collections.Generic;

namespace TpsParser.Tps
{
    public sealed class TpsPage
    {
        public int Address { get; }
        public int Size { get; }
        public int SizeUncompressed { get; }
        public int SizeUncompressedWithoutHeader { get; }
        public int RecordCount { get; }
        public int Flags { get; }

        private TpsReader CompressedData { get; }
        private List<TpsRecord> Records { get; }

        private TpsReader _data;

        private bool IsFlushed => _data is null;

        public TpsPage(TpsReader rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Records = new List<TpsRecord>();

            Address = rx.LongLE();
            Size = rx.ShortLE();

            var header = rx.Read(Size - 6);

            SizeUncompressed = header.ShortLE();
            SizeUncompressedWithoutHeader = header.ShortLE();
            RecordCount = header.ShortLE();
            Flags = header.Byte();

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
                        TpsRecord currentRecord = null;

                        if (previousRecord is null)
                        {
                            currentRecord = new TpsRecord(rx);
                        }
                        else
                        {
                            currentRecord = new TpsRecord(previousRecord, rx);
                        }

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

        public override string ToString() =>
            $"TpsPage({Address:X8},{Size:X4},{SizeUncompressed:X4},{SizeUncompressedWithoutHeader:X4},{RecordCount:X4},{Flags:X2})";
    }
}
