using System;
using System.Text;
using TpsParser.Binary;
using TpsParser.Tps.Header;
using TpsParser.TypeModel;

namespace TpsParser.Tps.Record;

/// <summary>
/// Encapsulates memo data. The data may be contained within a single record or segmented across multiple records.
/// See <see cref="IMemoHeader.SequenceNumber"/> when the memo is segmented.
/// </summary>
public interface IMemoRecord
{
    /// <summary>
    /// Gets the header for this particular record.
    /// </summary>
    IMemoHeader Header { get; }

    /// <summary>
    /// Gets the value of the memo using the given definition record.
    /// </summary>
    /// <param name="memoDefinitionRecord"></param>
    /// <returns></returns>
    ITpsObject GetValue(IMemoDefinitionRecord memoDefinitionRecord);
}

/// <inheritdoc/>
internal sealed class MemoRecord : IMemoRecord
{
    /// <inheritdoc/>
    public IMemoHeader Header { get; }

    private TpsRandomAccess Data { get; }

    /// <summary>
    /// Instantiates a new record.
    /// </summary>
    /// <param name="header"></param>
    /// <param name="rx"></param>
    public MemoRecord(IMemoHeader header, TpsRandomAccess rx)
    {
        Header = header ?? throw new ArgumentNullException(nameof(header));
        Data = rx ?? throw new ArgumentNullException(nameof(rx));
    }

    /// <inheritdoc/>
    public ITpsObject GetValue(IMemoDefinitionRecord memoDefinitionRecord)
    {
        ArgumentNullException.ThrowIfNull(memoDefinitionRecord);

        if (memoDefinitionRecord.IsBlob)
        {
            return Data.ReadBlob();
        }
        else
        {
            return Data.ReadMemo(Encoding.GetEncoding("ISO-8859-1"));
        }
    }
}
