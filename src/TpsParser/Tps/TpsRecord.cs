using System;
using System.Collections.Generic;
using System.Linq;
using TpsParser.Binary;

namespace TpsParser.Tps;

public sealed class TpsRecord
{
    public byte Flags { get; }
    public int RecordLength { get; }
    public int HeaderLength { get; }
    public TpsRandomAccess Data { get; }
    public IHeader Header { get; private set; }

    /// <summary>
    /// Creates a new <see cref="TpsRecord"/>. This is typically done on the first of a list.
    /// </summary>
    /// <param name="rx">The data to read from.</param>
    public TpsRecord(TpsRandomAccess rx)
    {
        if (rx == null)
        {
            throw new ArgumentNullException(nameof(rx));
        }

        Flags = rx.ReadByte();

        if ((Flags & 0xC0) != 0xC0)
        {
            throw new TpsParserException($"Cannot construct a TpsRecord without record lengths (0x{StringUtils.ToHex2(Flags)})");
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
    public TpsRecord(TpsRecord previous, TpsRandomAccess rx)
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
        newData.AddRange(rx.ReadBytesAsArray(RecordLength - copy));

        Data = new TpsRandomAccess(newData.ToArray(), rx.Encoding);

        if (Data.Length != RecordLength)
        {
            throw new TpsParserException("Data and record length mismatch.");
        }

        BuildHeader();
    }

    private void BuildHeader()
    {
        var rx = Data.Read(HeaderLength);

        if (rx.Length >= 5)
        {
            if (rx.PeekByte(0) == (byte)RecordType.TableName)
            {
                var preHeader = PreHeader.Parse(rx, readTableNumber: false);

                Header = TableNameHeader.Parse(preHeader, rx);
            }
            else
            {
                var preHeader = PreHeader.Parse(rx, readTableNumber: true);

                Header = preHeader.Type switch
                {
                    RecordType.TableName => TableNameHeader.Parse(preHeader, rx),
                    RecordType.Data => DataHeader.Parse(preHeader, rx),
                    RecordType.Metadata => MetadataHeader.Parse(preHeader, rx),
                    RecordType.TableDef => TableDefinitionHeader.Parse(preHeader, rx),
                    RecordType.Memo => MemoHeader.Parse(preHeader, rx),
                    RecordType.Index => IndexHeader.Parse(preHeader, rx),
                    _ => IndexHeader.Parse(preHeader, rx)
                };
            }
        }
    }
}
