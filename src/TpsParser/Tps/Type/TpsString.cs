using System;
using System.Text;
using TpsParser.Binary;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a fixed length string.
    /// </summary>
    public sealed class TpsString : TpsObject<string>
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.String;

        public TpsString(RandomAccess rx, int length, Encoding encoding)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            Value = rx.FixedLengthString(length, encoding);
        }

        public TpsString(RandomAccess rx, Encoding encoding)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            Value = encoding.GetString(rx.GetData());
        }
    }
}
