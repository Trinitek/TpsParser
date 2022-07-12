using System;
using System.Linq.Expressions;

namespace TpsParser
{
    /// <summary>
    /// Contains deserialization settings for boolean members.
    /// </summary>
    public sealed class BooleanOptions : TypeMapOptions
    {
        /// <summary>
        /// Gets or sets the value to use when testing for 'true',
        /// or <see cref="TypeMapOptions.UnsetValue"/> to use the default behavior.
        /// </summary>
        public object TrueValue { get; set; } = UnsetValue;

        /// <summary>
        /// Gets or sets the value to use when testing for 'false',
        /// or <see cref="TypeMapOptions.UnsetValue"/> to use the default behavior.
        /// </summary>
        public object FalseValue { get; set; } = UnsetValue;

        /// <summary>
        /// Gets or sets the fallback value to use when <see cref="TpsFieldAttribute.FallbackValue"/> is unset.
        /// </summary>
        public bool FallbackValue { get; set; } = false;

        /// <summary>
        /// Gets or sets the string comparison mode. The default is <see cref="StringComparison.OrdinalIgnoreCase"/>.
        /// </summary>
        public StringComparison ComparisonMode { get; set; } = StringComparison.OrdinalIgnoreCase;

        internal BooleanOptions GetCopy() => new BooleanOptions
        {
            TrueValue = TrueValue,
            FalseValue = FalseValue,
            ComparisonMode = ComparisonMode,
            FallbackValue = FallbackValue
        };

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0041:Use 'is null' check", Justification = "Cannot use pattern-matching operators in expression trees.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0075:Simplify conditional expression", Justification = "Nested ternaries have better readability, comparatively.")]
        protected internal override Expression<Func<TpsObject, object>> CreateValueInterpreter(object fallbackValue)
        {
            fallbackValue =
                fallbackValue == UnsetValue
                ? this.FallbackValue
                : fallbackValue;

            Expression<Func<TpsObject, object>> asBooleanOrFallbackExpr =
                x => ReferenceEquals(x, null) ? fallbackValue : x.ToBoolean().Value;

            if (TrueValue != UnsetValue && FalseValue == UnsetValue)
            {
                if (TrueValue is string trueString)
                {
                    return x =>
                        ReferenceEquals(x, null)
                        ? fallbackValue
                        : string.Equals(x.ToString().NullTrim(), trueString, ComparisonMode);
                }
                else
                {
                    return asBooleanOrFallbackExpr;
                }
            }
            else if (TrueValue == UnsetValue && FalseValue != UnsetValue)
            {
                if (FalseValue is string falseString)
                {
                    return x =>
                        ReferenceEquals(x, null)
                        ? fallbackValue
                        :
                            string.Equals(x.ToString().NullTrim(), falseString, ComparisonMode)
                            ? false
                            : x.ToBoolean().Value;
                }
                else
                {
                    return x =>
                        ReferenceEquals(x, null)
                        ? fallbackValue
                        : !Equals(x.Value, FalseValue);
                }
            }
            else if (TrueValue != UnsetValue && FalseValue != UnsetValue)
            {
                if (TrueValue is string trueString && FalseValue is string falseString)
                {
                    return x =>
                        ReferenceEquals(x, null)
                        ? fallbackValue
                        :
                            string.Equals(x.ToString().NullTrim(), trueString, ComparisonMode)
                            ? true
                            :
                                string.Equals(x.ToString().NullTrim(), falseString, ComparisonMode)
                                ? false
                                : fallbackValue;
                }
                else
                {
                    return x =>
                        ReferenceEquals(x, null)
                        ? fallbackValue
                        :
                            x.Value.Equals(TrueValue)
                            ? true
                            :
                                x.Value.Equals(FalseValue)
                                ? false
                                : fallbackValue;
                }
            }
            else
            {
                return asBooleanOrFallbackExpr;
            }
        }
    }
}
