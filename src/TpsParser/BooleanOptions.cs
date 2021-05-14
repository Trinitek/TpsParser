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
        /// Represents an unset value.
        /// </summary>
        public static object UnsetValue { get; } = new object();

        /// <summary>
        /// Gets or sets the value to use when testing for 'true',
        /// or <see cref="UnsetValue"/> to use the default behavior.
        /// </summary>
        public object TrueValue { get; set; } = UnsetValue;

        /// <summary>
        /// Gets or sets the value to use when testing for 'false',
        /// or <see cref="UnsetValue"/> to use the default behavior.
        /// </summary>
        public object FalseValue { get; set; } = UnsetValue;

        internal BooleanOptions GetCopy() => new BooleanOptions
        {
            TrueValue = TrueValue,
            FalseValue = FalseValue
        };
    }
}
