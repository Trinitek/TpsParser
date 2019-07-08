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
        public override string TypeName => "STRING";

        public override int TypeCode => 0x12;

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
    }
}
