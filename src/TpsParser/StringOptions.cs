using System;
using System.Linq.Expressions;
using TpsParser.Tps.Type;

namespace TpsParser
{
    /// <summary>
    /// Contains deserialization settings for string members.
    /// </summary>
    public sealed class StringOptions : TypeMapOptions
    {
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

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0041:Use 'is null' check", Justification = "Cannot use pattern-matching operators in expression trees.")]
        protected internal override Expression<Func<TpsObject, object>> CreateValueInterpreter(object fallbackValue)
        {
            if (Format is null)
            {
                if (TrimStart && !TrimEnd)
                {
                    return x =>
                        ReferenceEquals(x, null)
                        ? fallbackValue
                        : x.ToString().TrimStart();
                }
                else if (!TrimStart && TrimEnd)
                {
                    return x =>
                        ReferenceEquals(x, null)
                        ? fallbackValue
                        : x.ToString().TrimEnd();
                }
                else if (TrimStart && TrimEnd)
                {
                    return x =>
                        ReferenceEquals(x, null)
                        ? fallbackValue
                        : x.ToString().Trim();
                }
                else
                {
                    return x =>
                        ReferenceEquals(x, null)
                        ? fallbackValue
                        : x.ToString();
                }
            }
            else
            {
                if (TrimStart && !TrimEnd)
                {
                    return x =>
                        ReferenceEquals(x, null)
                        ? fallbackValue
                        : x.ToString(Format).TrimStart();
                }
                else if (!TrimStart && TrimEnd)
                {
                    return x =>
                        ReferenceEquals(x, null)
                        ? fallbackValue
                        : x.ToString(Format).TrimEnd();
                }
                else if (TrimStart && TrimEnd)
                {
                    return x =>
                        ReferenceEquals(x, null)
                        ? fallbackValue
                        : x.ToString(Format).Trim();
                }
                else
                {
                    return x =>
                        ReferenceEquals(x, null)
                        ? fallbackValue
                        : x.ToString(Format);
                }
            }
        }
    }
}
