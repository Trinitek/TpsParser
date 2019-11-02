using System;
using TpsParser.Binary;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents an unsigned integer.
    /// </summary>
    public sealed class TpsUnsignedLong : TpsObject<uint>
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.ULong;

        /// <summary>
        /// Instantiates a new ULONG.
        /// </summary>
        /// <param name="rx"></param>
        public TpsUnsignedLong(RandomAccess rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Value = rx.UnsignedLongLE();
        }

        /// <summary>
        /// Returns true if the value is not zero.
        /// </summary>
        internal override bool AsBoolean() => Value != 0;
    }
}
