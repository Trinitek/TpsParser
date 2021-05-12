using System;
using TpsParser.Tps.Type;

namespace TpsParser
{
    /// <summary>
    /// Contains deserialization settings for string members.
    /// </summary>
    public sealed class StringOptions
    {
        /// <summary>
        /// Gets a <see cref="StringOptions"/> instance with default options set.
        /// </summary>
        public static StringOptions Default { get; } = new StringOptions();

        /// <summary>
        /// True if whitespace at the beginning of the string should be trimmed.
        /// </summary>
        public bool TrimStart { get; set; } = false;

        /// <summary>
        /// True if whitespace at the end of the string should be trimmed.
        /// </summary>
        public bool TrimEnd { get; set; } = true;

        /// <summary>
        /// Gets or sets the custom string format to use where <see cref="IFormattable"/> is supported by the <see cref="TpsObject"/>.
        /// </summary>
        public string Format { get; set; } = null;

        internal StringOptions GetCopy() => new StringOptions
        {
            TrimStart = TrimStart,
            TrimEnd = TrimEnd,
            Format = Format
        };
    }
}
