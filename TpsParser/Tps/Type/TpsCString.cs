using System;
using System.Text;
using TpsParser.Binary;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a null-terminated string.
    /// </summary>
    public sealed class TpsCString : TpsObject<string>
    {
        public override string TypeName => "CSTRING";

        public override int TypeCode => 0x13;

        public TpsCString(RandomAccess rx, Encoding encoding)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            Value = rx.ZeroTerminatedString(encoding);
        }
    }
}
