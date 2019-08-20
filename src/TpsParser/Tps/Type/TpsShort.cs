using System;
using TpsParser.Binary;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a signed short.
    /// </summary>
    public sealed class TpsShort : TpsObject<short>
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.Short;

        /// <summary>
        /// Instantiates a new SHORT.
        /// </summary>
        /// <param name="rx"></param>
        public TpsShort(RandomAccess rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Value = rx.ShortLE();
        }

        /// <summary>
        /// Returns true if the value is not zero.
        /// </summary>
        protected override bool AsBoolean() => Value != 0;
    }
}
