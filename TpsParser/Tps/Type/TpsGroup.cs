using System;
using System.Collections.Generic;
using TpsParser.Binary;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a grouping of fields.
    /// </summary>
    public sealed class TpsGroup : TpsObject<IEnumerable<byte>>
    {
        public override string TypeName => "GROUP";

        public override int TypeCode => 0x16;

        public TpsGroup(RandomAccess rx, int length)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Value = rx.ReadBytes(length);
        }
    }
}
