using System;
using TpsParser.Tps.Type;

namespace TpsParser
{
    /// <summary>
    /// <para>
    /// Marks the property or field as a TopSpeed field, MEMO, or BLOB. This attribute is intended for use on <see cref="string"/>
    /// members to provide trimming and formatting options. Strings are trimmed by default unless explicitly disabled.
    /// </para>
    /// <para>
    /// If present on a field, the field may be private.
    /// </para>
    /// <para>
    /// If present on a property, the property must have a setter. The setter may be private.
    /// </para>
    /// </summary>
    public sealed class StringOptionsAttribute : TpsFieldAttribute
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

        /// <summary>
        /// Marks the property or field as a TopSpeed field, MEMO, or BLOB.
        /// </summary>
        /// <param name="fieldName">The case insensitive name of the column.</param>
        public StringOptionsAttribute(string fieldName)
            : base(fieldName)
        { }

        internal StringOptions GetOptions() => new StringOptions
        {
            TrimStart = TrimStart,
            TrimEnd = TrimEnd,
            Format = StringFormat
        };

        internal override object InterpretValue(Type memberType, TpsObject sourceObject)
        {
            if (memberType is null)
            {
                throw new ArgumentNullException(nameof(memberType));
            }

            if (memberType != typeof(string))
            {
                throw new TpsParserException($"{nameof(StringOptionsAttribute)} is only valid on members of type {typeof(string)}.");
            }

            if (sourceObject?.Value is null)
            {
                return FallbackValue;
            }

            string result
                = StringFormat is null
                ? sourceObject.ToString()
                : sourceObject.ToString(StringFormat);

            if (TrimEnd)
            {
                result = result?.TrimEnd();
            }

            return result;
        }
    }
}
