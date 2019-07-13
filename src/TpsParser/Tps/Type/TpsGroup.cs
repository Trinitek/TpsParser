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
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.Group;

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
