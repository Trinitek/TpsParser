﻿using System;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a MEMO containing an ISO-8859-1 encoded string. MEMOs are variable-length
    /// text fields that can be up to 65,536 bytes long.
    /// </summary>
    public sealed class TpsMemo : TpsObject<string>
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.None;

        /// <summary>
        /// Instantiates a new MEMO.
        /// </summary>
        /// <param name="rx"></param>
        public TpsMemo(TpsReader rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Value = TpsParser.DefaultEncoding.GetString(rx.GetData());
        }

        /// <summary>
        /// Returns true if the text does not have a length of zero.
        /// </summary>
        public override Maybe<bool> ToBoolean() => new Maybe<bool>(!string.IsNullOrEmpty(Value));
    }
}
