using System;
using System.Text;
using TpsParser.Binary;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a memo containing an ISO-8859-1 encoded string.
    /// </summary>
    public sealed class TpsMemo : TpsObject<string>
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.Memo;

        public TpsMemo(RandomAccess rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Value = Encoding.GetEncoding("ISO-8859-1").GetString(rx.GetData());
        }
    }
}
