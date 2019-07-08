using System;
using System.Text;
using TpsParser.Binary;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a Pascal string where the length is specified at the beginning of the string.
    /// </summary>
    public sealed class TpsPString : TpsObject<string>
    {
        public override string TypeName => "PSTRING";

        public override int TypeCode => 0x14;

        public TpsPString(RandomAccess rx, Encoding encoding)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            Value = rx.PascalString(encoding);
        }
    }
}
