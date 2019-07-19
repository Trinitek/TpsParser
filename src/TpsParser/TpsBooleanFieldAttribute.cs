using TpsParser.Tps.Type;

namespace TpsParser
{
    /// <summary>
    /// <para>
    /// Marks the property or field as a TopSpeed field, MEMO, or BLOB. This attribute is intended for use on <see cref="bool"/>
    /// members to provide conversion options for interpreting TopSpeed field values as true or false.
    /// </para>
    /// <para>
    /// This is especially useful when converting from string fields that represent boolean values in a non-conventional way that
    /// cannot be automatically inferred by <see cref="TpsFieldAttribute"/>. For example, a STRING(1) field might encode "Y" as
    /// true and "N" as false.
    /// </para>
    /// <para>
    /// The default behavior for value conversions to <see cref="bool"/> are described on the overrides of <see cref="TpsObject.AsBoolean"/>
    /// for each implementing type.
    /// </para>
    /// </summary>
    public sealed class TpsBooleanFieldAttribute : TpsFieldAttribute
    {
        /// <summary>
        /// Gets or sets the value to interpret as true. The default behavior is used unless otherwise specified.
        /// When this is a string, a case-insensitive comparison is made.
        /// </summary>
        public object TrueValue { get; set; } = Behavior.Default;

        /// <summary>
        /// Gets or sets the value to interpret as false. The default behavior is used unless otherwise specified.
        /// When this is a string, a case-insensitive comparison is made.
        /// </summary>
        public object FalseValue { get; set; } = Behavior.Default;

        /// <inheritdoc/>
        public TpsBooleanFieldAttribute(string fieldName)
            : base(fieldName)
        {
            FallbackValue = false;
        }
    }
}
