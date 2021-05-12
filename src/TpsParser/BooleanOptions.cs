using TpsParser.Tps.Type;

namespace TpsParser
{
    /// <summary>
    /// Contains deserialization settings for boolean members.
    /// </summary>
    public sealed class BooleanOptions
    {
        /// <summary>
        /// Gets a <see cref="BooleanOptions"/> instance with default options set.
        /// </summary>
        public static BooleanOptions Default { get; } = new BooleanOptions();

        /// <summary>
        /// Gets or sets the value to use when testing for 'true',
        /// or null to use the default behavior.
        /// </summary>
        public TpsObject TrueValue { get; set; } = null;

        /// <summary>
        /// Gets or sets the value to use when testing for 'false',
        /// or null to use the default behavior.
        /// </summary>
        public TpsObject FalseValue { get; set; } = null;

        internal BooleanOptions GetCopy() => new BooleanOptions
        {
            TrueValue = TrueValue,
            FalseValue = FalseValue
        };
    }
}
