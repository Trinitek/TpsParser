using System;

namespace TpsParser.Tps.Record;

public sealed class IndexRecord
{
    private IndexHeader Header { get; }
    public int RecordNumber { get; }

    public IndexRecord(TpsRecord tpsRecord)
    {
        if (tpsRecord == null)
        {
            throw new ArgumentNullException(nameof(tpsRecord));
        }

        Header = (IndexHeader)tpsRecord.Header;

        var data = tpsRecord.DataRx;
        data.JumpAbsolute(data.Length - 4);

        RecordNumber = data.ReadLongBE();
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"IndexRecord({Header.TableNumber},{Header.IndexNumber},{RecordNumber})";
}
