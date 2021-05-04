﻿using System;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents an unsigned short.
    /// </summary>
    public sealed class TpsUnsignedShort : TpsObject<ushort>
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.UShort;

        /// <summary>
        /// Instantiates a new SHORT.
        /// </summary>
        /// <param name="rx"></param>
        public TpsUnsignedShort(TpsReader rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Value = rx.UnsignedShortLE();
        }

        /// <summary>
        /// Returns true if the value is not zero.
        /// </summary>
        internal override bool AsBoolean() => Value != 0;
    }
}
