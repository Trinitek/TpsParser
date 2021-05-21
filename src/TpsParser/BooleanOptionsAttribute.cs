using System;
using TpsParser.Tps.Type;

namespace TpsParser
{
    /// <summary>
    /// <para>
    /// Specifies additional deserialization settings for boolean members.
    /// </para>
    /// <para>
    /// This attribute is useful when converting from string fields that represent boolean values in a non-conventional way that
    /// cannot be automatically inferred by <see cref="TpsFieldAttribute"/>. For example, a STRING(1) field might encode "Y" as
    /// true and "N" as false.
    /// </para>
    /// <para>
    /// Where the field is of type <see cref="TpsString"/>, <see cref="TpsPString"/>, or <see cref="TpsCString"/>,
    /// both leading and trailing whitespace is trimmed before comparison.
    /// </para>
    /// <para>
    /// The default behavior for value conversions to <see cref="bool"/> are described on the overrides of <see cref="TpsObject.ToBoolean"/>
    /// for each implementing type.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class BooleanOptionsAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the value to interpret as true. The default behavior is used unless otherwise specified.
        /// When this is a string, a case-insensitive comparison is made.
        /// </summary>
        public object TrueValue { get; set; } = TypeMapOptions.UnsetValue;

        /// <summary>
        /// Gets or sets the value to interpret as false. The default behavior is used unless otherwise specified.
        /// When this is a string, a case-insensitive comparison is made.
        /// </summary>
        public object FalseValue { get; set; } = TypeMapOptions.UnsetValue;

        /// <summary>
        /// Gets or sets the string comparison mode. The default is <see cref="StringComparison.OrdinalIgnoreCase"/>.
        /// </summary>
        public StringComparison ComparisonMode { get; set; } = StringComparison.OrdinalIgnoreCase;

        internal BooleanOptions GetOptions() => new BooleanOptions
        {
            TrueValue = TrueValue,
            FalseValue = FalseValue,
            ComparisonMode = ComparisonMode
        };
    }
}
