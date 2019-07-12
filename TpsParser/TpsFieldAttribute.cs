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
    public sealed class TpsFieldAttribute : Attribute
    {
        public string FieldName { get; }

        public object FallbackValue { get; }

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
