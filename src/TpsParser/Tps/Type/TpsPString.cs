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
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.PString;

        /// <summary>
        /// Instantiates a new PSTRING.
        /// </summary>
        /// <param name="rx"></param>
        /// <param name="encoding"></param>
        public TpsPString(TpsRandomAccess rx, Encoding encoding)
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

        /// <summary>
        /// Instantiate a new PSTRING.
        /// </summary>
        /// <param name="value">The string value. Must not be null.</param>
        public TpsPString(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Returns true if the string length is not zero.
        /// </summary>
        protected override bool AsBoolean() => Value.Length > 0;
    }
}
