using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Instantiates a new GROUP.
        /// </summary>
        /// <param name="rx"></param>
        /// <param name="length"></param>
        public TpsGroup(RandomAccess rx, int length)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Value = rx.PeekBytes(length);
        }

        /// <summary>
        /// Returns true if the data size is not zero.
        /// </summary>
        protected override bool AsBoolean() => Value.Any();
    }
}
