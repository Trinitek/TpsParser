using System;
using System.Text;
using TpsParser.Tps.Header;
using TpsParser.Tps.Type;

namespace TpsParser.Tps.Record
{
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
        TpsObject GetValue(IMemoDefinitionRecord memoDefinitionRecord);
    }

    /// <inheritdoc/>
    internal sealed class MemoRecord : IMemoRecord
    {
        /// <inheritdoc/>
        public IMemoHeader Header { get; }

        private TpsReader Data { get; }

        /// <summary>
        /// Instantiates a new record.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="rx"></param>
        public MemoRecord(IMemoHeader header, TpsReader rx)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));
            Data = rx ?? throw new ArgumentNullException(nameof(rx));
        }

        /// <inheritdoc/>
        public TpsObject GetValue(IMemoDefinitionRecord memoDefinitionRecord)
        {
            if (memoDefinitionRecord == null)
            {
                throw new ArgumentNullException(nameof(memoDefinitionRecord));
            }

            if (memoDefinitionRecord.IsBlob)
            {
                return new TpsBlob(Data);
            }
            else
            {
                return new TpsString(Data, Encoding.GetEncoding("ISO-8859-1"));
            }
        }
    }
}
