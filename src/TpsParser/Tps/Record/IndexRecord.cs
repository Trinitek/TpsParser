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

        // Payload data contains, in order, each field value in the index, followed by the record number.
        //
        // Descending key fields are encoded such that
        // Int32 79293 is encoded as (int.MaxValue - 79293) which is 0x7FFECA42
        // To get 79293 again, unchecked(-(IndexValue - int.MaxValue))

        var data = tpsRecord.DataRx;
        data.JumpAbsolute(data.Length - 4);

        RecordNumber = data.ReadLongBE();
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"IndexRecord({Header.TableNumber},{Header.IndexNumber},{RecordNumber})";
}
