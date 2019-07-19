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
        /// Returns true if the end of the string should be trimmed. This is useful when converting from <see cref="TpsString"/>
        /// values as those strings are padded with whitespace up to their total lengths. This is true by default.
        /// </summary>
        public bool TrimEnd { get; }

        /// <summary>
        /// Gets the string format to use when calling ToString() on a non-string type.
        /// </summary>
        public string StringFormat { get; }

        /// <summary>
        /// Marks the property or field as a TopSpeed field, MEMO, or BLOB.
        /// </summary>
        /// <param name="fieldName">The case insensitive name of the column.</param>
        /// <param name="fallbackValue">Fallback value to use if the field is null. The fallback value is applied after any necessary ToString() conversion and trimming.</param>
        /// <param name="isRequired">
        /// <para>
        /// Throw an exception if the field is not found during deserialization.
        /// </para>
        /// <para>
        /// Note that a field might be be present in some rows and missing in others. This is especially true for MEMOs and BLOBs.
        /// </para>
        /// </param>
        /// <param name="trimEnd">True if the end of the string should be trimmed.</param>
        /// <param name="stringFormat">The string format to use when calling ToString() on a non-string type.</param>
        public TpsStringFieldAttribute(string fieldName, object fallbackValue = null, bool isRequired = false, bool trimEnd = true, string stringFormat = null)
            : base(fieldName, fallbackValue, isRequired)
        {
            TrimEnd = trimEnd;
            StringFormat = stringFormat;
        }
    }
}
