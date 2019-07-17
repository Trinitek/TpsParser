using System;

namespace TpsParser
{
    /// <summary>
    /// <para>
    /// Marks the property or field as a TopSpeed field, MEMO, or BLOB.
    /// </para>
    /// <para>
    /// If present on a field, the field may be private.
    /// </para>
    /// <para>
    /// If present on a property, the property must have a setter. The setter may be private.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class TpsFieldAttribute : Attribute
    {
        /// <summary>
        /// Gets the case-insensitive name of the column.
        /// </summary>
        public string FieldName { get; }

        /// <summary>
        /// Gets the fallback value to use if the field is null.
        /// </summary>
        public object FallbackValue { get; }

        /// <summary>
        /// Returns true if the deserializer should throw a <see cref="TpsParserException"/> if the column is not found on the row.
        /// </summary>
        public bool IsRequired { get; }

        /// <summary>
        /// Marks the property or field as a TopSpeed field, MEMO, or BLOB.
        /// </summary>
        /// <param name="fieldName">The case insensitive name of the column.</param>
        /// <param name="fallbackValue">Fallback value to use if the field is null.</param>
        /// <param name="isRequired">
        /// <para>
        /// Throw an exception if the field is not found during deserialization.
        /// </para>
        /// <para>
        /// Note that a field might be be present in some rows and missing in others. This is especially true for MEMOs and BLOBs.
        /// </para>
        /// </param>
        public TpsFieldAttribute(string fieldName, object fallbackValue = null, bool isRequired = false)
        {
            FieldName = fieldName;
            FallbackValue = fallbackValue;
            IsRequired = isRequired;
        }
    }
}
