using System;
using System.Text;
using TpsParser.Binary;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a null-terminated string.
    /// </summary>
    public sealed class TpsCString : TpsObject<string>
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.CString;

        /// <summary>
        /// Instantiates a new null-terminated string.
        /// </summary>
        /// <param name="rx">The binary reader containing the raw string data.</param>
        /// <param name="encoding">The text encoding from which to create a well-formed string.</param>
        public TpsCString(RandomAccess rx, Encoding encoding)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            Value = rx.ZeroTerminatedString(encoding);
        }

        /// <summary>
        /// Instantiates a new CSTRING.
        /// </summary>
        /// <param name="value">The string value. Must not be null.</param>
        public TpsCString(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Returns true if the string's length is greater than zero.
        /// </summary>
        protected override bool AsBoolean() => Value.Length > 0;
    }
}
