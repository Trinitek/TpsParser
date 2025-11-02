using System;
using TpsParser.Binary;
using TpsParser.Tps.Header;
using TpsParser.TypeModel;

namespace TpsParser.Tps.Record;

/// <summary>
/// Encapsulates memo data. The data may be contained within a single record or segmented across multiple records.
/// See <see cref="IMemoHeader.SequenceNumber"/> when the memo is segmented.
/// </summary>
public sealed class MemoRecord
{
    private readonly TpsRandomAccess _data;

    /// <summary>
    /// Gets the header for this particular record.
    /// </summary>
    public IMemoHeader Header { get; }

    /// <summary>
    /// Instantiates a new record.
    /// </summary>
    /// <param name="header"></param>
    /// <param name="rx"></param>
    public MemoRecord(IMemoHeader header, TpsRandomAccess rx)
    {
        Header = header ?? throw new ArgumentNullException(nameof(header));
        _data = rx ?? throw new ArgumentNullException(nameof(rx));
    }

    /// <summary>
    /// Gets the value of the memo using the given definition record.
    /// </summary>
    /// <param name="memoDefinitionRecord"></param>
    /// <returns></returns>
    public ITpsObject GetValue(MemoDefinitionRecord memoDefinitionRecord)
    {
        ArgumentNullException.ThrowIfNull(memoDefinitionRecord);

        if (memoDefinitionRecord.IsBlob)
        {
            return _data.ReadBlob();
        }
        else
        {
            return _data.ReadMemo();
        }
    }
}
