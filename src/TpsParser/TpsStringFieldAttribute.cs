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
    public sealed class TpsStringFieldAttribute : TpsFieldAttribute
    {
        /// <summary>
        /// True if the end of the string should be trimmed. This is useful when converting from <see cref="TpsString"/>
        /// values as those strings are padded with whitespace up to their total lengths. This is true by default.
        /// </summary>
        public bool TrimEnd { get; set; } = true;

        /// <summary>
        /// Gets or sets the string format to use when calling ToString() on a non-string type.
        /// </summary>
        public string StringFormat { get; set; }

        /// <summary>
        /// Marks the property or field as a TopSpeed field, MEMO, or BLOB.
        /// </summary>
        /// <param name="fieldName">The case insensitive name of the column.</param>
        public TpsStringFieldAttribute(string fieldName)
            : base(fieldName)
        { }
    }
}
