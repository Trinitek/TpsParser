using System;
using System.Linq.Expressions;
using TpsParser.Tps.Type;

namespace TpsParser
{
    /// <summary>
    /// Contains deserialization settings for boolean members.
    /// </summary>
    public sealed class BooleanOptions : TypeMapOptions
    {
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

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0041:Use 'is null' check", Justification = "Cannot use pattern-matching operators in expression trees.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0075:Simplify conditional expression", Justification = "Nested ternaries have better readability, comparatively.")]
        protected internal override Expression<Func<TpsObject, object>> CreateValueInterpreter(object fallbackValue)
        {
            Expression<Func<TpsObject, object>> asBooleanOrFallbackExpr =
                x => ReferenceEquals(x, null) ? fallbackValue : x.ToBoolean().Value;

            if (TrueValue != UnsetValue && FalseValue == UnsetValue)
            {
                if (TrueValue is string trueString)
                {
                    return x =>
                        ReferenceEquals(x, null)
                        ? fallbackValue
                        : string.Equals(x.ToString().Trim(), trueString, StringComparison.OrdinalIgnoreCase);
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
                            string.Equals(x.ToString().Trim(), falseString, StringComparison.OrdinalIgnoreCase)
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
                            string.Equals(x.ToString().Trim(), trueString, StringComparison.OrdinalIgnoreCase)
                            ? true
                            :
                                string.Equals(x.ToString().Trim(), falseString, StringComparison.OrdinalIgnoreCase)
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
