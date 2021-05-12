using System;
using System.Collections.Generic;
using System.Linq;
using TpsParser.Tps.Header;

namespace TpsParser.Tps
{
    public sealed class TpsRecord
    {
        public byte Flags { get; }

        /// <summary>
        /// Gets the length of the record in bytes.
        /// </summary>
        public int RecordLength { get; }

        /// <summary>
        /// Gets the length of the header in bytes.
        /// </summary>
        public int HeaderLength { get; }
        public TpsReader Data { get; }
        public IHeader Header { get; private set; }

        /// <summary>
        /// Creates a new <see cref="TpsRecord"/>. This is typically done on the first of a list.
        /// </summary>
        /// <param name="rx">The data to read from.</param>
        public TpsRecord(TpsReader rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Flags = rx.ReadByte();

            if ((Flags & 0xC0) != 0xC0)
            {
                throw new ArgumentException($"Cannot construct a TpsRecord without record lengths (0x{StringUtils.ToHex2(Flags)})");
            }

            RecordLength = rx.ReadShortLE();
            HeaderLength = rx.ReadShortLE();

            Data = rx.Read(RecordLength);

            BuildHeader();
        }

        /// <summary>
        /// Creates a new <see cref="TpsRecord"/> by partially copying the previous one.
        /// </summary>
        /// <param name="previous">The previous record.</param>
        /// <param name="rx">The data to read from.</param>
        public TpsRecord(TpsRecord previous, TpsReader rx)
        {
            if (previous == null)
            {
                throw new ArgumentNullException(nameof(previous));
            }

            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Flags = rx.ReadByte();

            if ((Flags & 0x80) != 0)
            {
                RecordLength = rx.ReadShortLE();
            }
            else
            {
                RecordLength = previous.RecordLength;
            }

            if ((Flags & 0x40) != 0)
            {
                HeaderLength = rx.ReadShortLE();
            }
            else
            {
                HeaderLength = previous.HeaderLength;
            }

            int copy = Flags & 0x3F;

            var newData = new List<byte>(RecordLength);

            newData.AddRange(previous.Data.GetData().Take(copy));
            newData.AddRange(rx.ReadBytes(RecordLength - copy));

            Data = new TpsReader(newData.ToArray());

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
                if ((HeaderKind)rx.Peek(0) == HeaderKind.TableName)
                {
                    Header = TableNameHeader.Read(rx);
                }
                else
                {
                    switch ((HeaderKind)rx.Peek(4))
                    {
                        case HeaderKind.Data:
                            Header = DataHeader.Read(rx);
                            break;
                        case HeaderKind.Metadata:
                            Header = MetadataHeader.Read(rx);
                            break;
                        case HeaderKind.TableDefinition:
                            Header = TableDefinitionHeader.Read(rx);
                            break;
                        case HeaderKind.Memo:
                            Header = MemoHeader.Read(rx);
                            break;
                        default:
                            Header = IndexHeader.Read(rx);
                            break;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override string ToString() =>
            $"TpsRecord(L:{RecordLength},H:{HeaderLength},{Header})";
    }
}
