using System;
using System.Text;
using TpsParser.Binary;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a fixed length string.
    /// </summary>
    public sealed class TpsString : TpsObject<string>
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.String;

        /// <summary>
        /// Instantiates a new STRING from the given binary reader.
        /// </summary>
        /// <param name="rx">The reader from which to read the string data.</param>
        /// <param name="length">The length of the string.</param>
        /// <param name="encoding">The encoding to use to decode the string data.</param>
        public TpsString(TpsReader rx, int length, Encoding encoding)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            Value = rx.FixedLengthString(length, encoding);
        }

        /// <summary>
        /// Instantiates a new STRING from the given binary reader.
        /// </summary>
        /// <param name="rx">The reader from which to read the raw string data.</param>
        /// <param name="encoding">The encoding to use to decode the string data.</param>
        public TpsString(TpsReader rx, Encoding encoding)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            Value = encoding.GetString(rx.GetData());
        }

        /// <summary>
        /// Instantiates a new STRING from the given value.
        /// </summary>
        /// <param name="value">The string value. Must not be null.</param>
        public TpsString(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Returns true if the string contains more than only whitespace.
        /// </summary>
        protected override bool AsBoolean() => !string.IsNullOrWhiteSpace(Value);
    }
}
