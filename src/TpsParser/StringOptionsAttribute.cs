using System;
using TpsParser.Tps.Type;

namespace TpsParser
{
    /// <summary>
    /// <para>
    /// Specifies additional deserialization settings for string members.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class StringOptionsAttribute : Attribute
    {
        /// <summary>
        /// True if the beginning of the string should be trimmed.
        /// </summary>
        public bool TrimStart { get; set; } = false;

        /// <summary>
        /// True if the end of the string should be trimmed. This is useful when converting from <see cref="TpsString"/>
        /// values as those strings are padded with whitespace up to their total lengths. This is true by default.
        /// </summary>
        public bool TrimEnd { get; set; } = true;

        /// <summary>
        /// Gets or sets the string format to use when calling ToString() on a type that supports it. The invariant culture is used.
        /// If the type does not support a custom format, this value is ignored.
        /// </summary>
        public string StringFormat { get; set; } = null;

        internal StringOptions GetOptions() => new StringOptions
        {
            TrimStart = TrimStart,
            TrimEnd = TrimEnd,
            Format = StringFormat
        };
    }
}
