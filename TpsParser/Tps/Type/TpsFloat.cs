using System;
using TpsParser.Binary;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a single-precision floating point number.
    /// </summary>
    public sealed class TpsFloat : TpsObject<float>
    {
        public override string TypeName => "BFLOAT4";

        public override int TypeCode => 0x08;

        public TpsFloat(RandomAccess rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Value = rx.FloatLE();
        }
    }
}
