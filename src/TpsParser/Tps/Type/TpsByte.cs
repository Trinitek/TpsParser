using System;
using TpsParser.Binary;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a byte.
    /// </summary>
    public sealed class TpsByte : TpsObject<byte>
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.Byte;

        /// <summary>
        /// Instantiates a new BYTE from the given binary reader.
        /// </summary>
        /// <param name="rx"></param>
        public TpsByte(RandomAccess rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Value = rx.Byte();
        }

        /// <summary>
        /// Returns true if the value is not zero.
        /// </summary>
        internal override bool AsBoolean() => Value != 0;
    }
}
