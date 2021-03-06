﻿using System;
using System.Collections.Generic;
using System.Linq;
using TpsParser.Binary;
using TpsParser.Tps.Header;

namespace TpsParser.Tps
{
    public sealed class TpsRecord
    {
        public int Flags { get; }
        public int RecordLength { get; }
        public int HeaderLength { get; }
        public RandomAccess Data { get; }
        public IHeader Header { get; private set; }

        /// <summary>
        /// Creates a new <see cref="TpsRecord"/>. This is typically done on the first of a list.
        /// </summary>
        /// <param name="rx">The data to read from.</param>
        public TpsRecord(RandomAccess rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Flags = rx.Byte();

            if ((Flags & 0xC0) != 0xC0)
            {
                throw new ArgumentException($"Cannot construct a TpsRecord without record lengths (0x{rx.ToHex2(Flags)})");
            }

            RecordLength = rx.ShortLE();
            HeaderLength = rx.ShortLE();

            Data = rx.Read(RecordLength);

            BuildHeader();
        }

        /// <summary>
        /// Creates a new <see cref="TpsRecord"/> by partially copying the previous one.
        /// </summary>
        /// <param name="previous">The previous record.</param>
        /// <param name="rx">The data to read from.</param>
        public TpsRecord(TpsRecord previous, RandomAccess rx)
        {
            if (previous == null)
            {
                throw new ArgumentNullException(nameof(previous));
            }

            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Flags = rx.Byte();

            if ((Flags & 0x80) != 0)
            {
                RecordLength = rx.ShortLE();
            }
            else
            {
                RecordLength = previous.RecordLength;
            }

            if ((Flags & 0x40) != 0)
            {
                HeaderLength = rx.ShortLE();
            }
            else
            {
                HeaderLength = previous.HeaderLength;
            }

            int copy = Flags & 0x3F;

            var newData = new List<byte>(RecordLength);

            newData.AddRange(previous.Data.GetData().Take(copy));
            newData.AddRange(rx.ReadBytes(RecordLength - copy));

            Data = new RandomAccess(newData.ToArray());

            if (Data.Length != RecordLength)
            {
                throw new ArgumentException("Data and record length mismatch.");
            }

            BuildHeader();
        }

        private void BuildHeader()
        {
            var rx = Data.Read(HeaderLength);

            if (rx.Length >= 5)
            {
                if (rx.Peek(0) == 0xFE)
                {
                    Header = new TableNameHeader(rx);
                }
                else
                {
                    switch (rx.Peek(4))
                    {
                        case 0xF3:
                            Header = new DataHeader(rx);
                            break;
                        case 0xF6:
                            Header = new MetadataHeader(rx);
                            break;
                        case 0xFA:
                            Header = new TableDefinitionHeader(rx);
                            break;
                        case 0xFC:
                            Header = new MemoHeader(rx);
                            break;
                        default:
                            Header = new IndexHeader(rx);
                            break;
                    }
                }
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override string ToString() =>
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
            $"TpsRecord(L:{RecordLength},H:{HeaderLength},{Header})";
    }
}
