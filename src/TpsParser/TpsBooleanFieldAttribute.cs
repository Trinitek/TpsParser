using System;
using System.Reflection;
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
    /// Where the field is of type STRING, both leading and trailing whitespace is trimmed before comparison.
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

        private bool? AsBoolean(TpsObject sourceObject) => ((IConvertible<bool>)sourceObject)?.AsType();

        internal override object InterpretValue(MemberInfo member, TpsObject sourceObject)
        {
            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            Type memberType = GetMemberType(member);

            if (memberType != typeof(bool) && memberType != typeof(bool?))
            {
                throw new TpsParserException($"{nameof(TpsBooleanFieldAttribute)} is only valid on members of type {typeof(bool)} ({member}).");
            }

            var tpsValue = sourceObject?.Value;

            if (TrueValue != Behavior.Default && FalseValue == Behavior.Default)
            {
                if (tpsValue is string tpsString && TrueValue is string trueString)
                {
                    return string.Equals(tpsString.Trim(), trueString, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    return AsBoolean(sourceObject) ?? FallbackValue;
                }
            }
            else if (TrueValue == Behavior.Default && FalseValue != Behavior.Default)
            {
                if (tpsValue is string tpsString && FalseValue is string falseString)
                {
                    bool isFalse = string.Equals(tpsString.Trim(), falseString, StringComparison.OrdinalIgnoreCase);

                    return isFalse ? false : AsBoolean(sourceObject) ?? FallbackValue;
                }
                else
                {
                    bool isFalse = tpsValue?.Equals(FalseValue) == false;

                    return isFalse ? false : AsBoolean(sourceObject) ?? FallbackValue;
                }
            }
            else if (TrueValue != Behavior.Default && FalseValue != Behavior.Default)
            {
                if (tpsValue is string tpsString && TrueValue is string trueString && FalseValue is string falseString)
                {
                    if (string.Equals(tpsString.Trim(), trueString, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    else if (string.Equals(tpsString.Trim(), falseString, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                    else
                    {
                        return FallbackValue;
                    }
                }
                else
                {
                    if (tpsValue?.Equals(TrueValue) == true)
                    {
                        return true;
                    }
                    else if (tpsValue?.Equals(FalseValue) == true)
                    {
                        return false;
                    }
                    else
                    {
                        return FallbackValue;
                    }
                }
            }
            else
            {
                return AsBoolean(sourceObject) ?? FallbackValue;
            }
        }
    }
}
