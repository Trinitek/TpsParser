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

        private static object ValueOrFallback(string s, object fallback) => s is null ? fallback : s;

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0041:Use 'is null' check", Justification = "Cannot use pattern-matching operators in expression trees.")]
        protected internal override Expression<Func<ITpsObject, object>> CreateValueInterpreter(object fallbackValue)
        {
            return null;

            //if (Format is null)
            //{
            //    if (TrimStart && !TrimEnd)
            //    {
            //        return x =>
            //            ReferenceEquals(x, null)
            //            ? fallbackValue
            //            : ValueOrFallback(x.ToString().NullTrimStart(), fallbackValue);
            //    }
            //    else if (!TrimStart && TrimEnd)
            //    {
            //        return x =>
            //            ReferenceEquals(x, null)
            //            ? fallbackValue
            //            : ValueOrFallback(x.ToString().NullTrimEnd(), fallbackValue);
            //    }
            //    else if (TrimStart && TrimEnd)
            //    {
            //        return x =>
            //            ReferenceEquals(x, null)
            //            ? fallbackValue
            //            : ValueOrFallback(x.ToString().NullTrim(), fallbackValue);
            //    }
            //    else
            //    {
            //        return x =>
            //            ReferenceEquals(x, null)
            //            ? fallbackValue
            //            : ValueOrFallback(x.ToString(), fallbackValue);
            //    }
            //}
            //else
            //{
            //    if (TrimStart && !TrimEnd)
            //    {
            //        return x =>
            //            ReferenceEquals(x, null)
            //            ? fallbackValue
            //            : ValueOrFallback(x.ToString(Format).NullTrimStart(), fallbackValue);
            //    }
            //    else if (!TrimStart && TrimEnd)
            //    {
            //        return x =>
            //            ReferenceEquals(x, null)
            //            ? fallbackValue
            //            : ValueOrFallback(x.ToString(Format).NullTrimEnd(), fallbackValue);
            //    }
            //    else if (TrimStart && TrimEnd)
            //    {
            //        return x =>
            //            ReferenceEquals(x, null)
            //            ? fallbackValue
            //            : ValueOrFallback(x.ToString(Format).NullTrim(), fallbackValue);
            //    }
            //    else
            //    {
            //        return x =>
            //            ReferenceEquals(x, null)
            //            ? fallbackValue
            //            : ValueOrFallback(x.ToString(Format), fallbackValue);
            //    }
            //}
        }
    }
}
