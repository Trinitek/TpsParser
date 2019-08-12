using System;
using System.Reflection;
using TpsParser.Tps.Type;

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
        /// Gets or sets the fallback value to use if the field is null.
        /// </summary>
        public object FallbackValue { get; set; } = null;

        /// <summary>
        /// <para>
        /// True if the deserializer should throw a <see cref="TpsParserException"/> if the column is not found on the row. This is false by default.
        /// </para>
        /// <para>
        /// Note that a field might be be present in some rows and missing in others. This is especially true for MEMOs and BLOBs.
        /// </para>
        /// </summary>
        public bool IsRequired { get; set; } = false;

        /// <summary>
        /// Marks the property or field as a TopSpeed field, MEMO, or BLOB.
        /// </summary>
        /// <param name="fieldName">The case insensitive name of the column.</param>
        public TpsFieldAttribute(string fieldName)
        {
            FieldName = fieldName;
        }
        internal static Type GetMemberType(MemberInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            switch (info)
            {
                case PropertyInfo propInfo:
                    return propInfo.PropertyType;
                case FieldInfo fieldInfo:
                    return fieldInfo.FieldType;
                default:
                    throw new TpsParserException($"Tried to get the type of an unsupported member on the target deserialization class ({info}).");
            }
        }

        internal virtual object InterpretValue(MemberInfo member, TpsObject sourceObject)
        {
            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            Type memberType = GetMemberType(member);

            var interpretedValue = InterpretValuePrivate(memberType, sourceObject);

            return CoerceFallback(interpretedValue);
        }

        private static object InterpretValuePrivate(Type memberType, TpsObject sourceObject)
        {
            if (memberType == typeof(DateTime) || memberType == typeof(DateTime?))
            {
                if (sourceObject is TpsLong longSource)
                {
                    return longSource.AsDate();
                }
                else
                {
                    return sourceObject?.Value;
                }
            }
            else if (memberType == typeof(TimeSpan) || memberType == typeof(TimeSpan?))
            {
                if (sourceObject is TpsLong longSource)
                {
                    return longSource.AsTime();
                }
                else
                {
                    return sourceObject?.Value;
                }
            }
            else if (memberType == typeof(bool) || memberType == typeof(bool?))
            {
                return sourceObject?.AsBoolean();
            }
            else if (memberType == typeof(string))
            {
                return sourceObject?.ToString();
            }
            else if (memberType == typeof(decimal) || memberType == typeof(decimal?))
            {
                if (sourceObject is TpsDecimal decimalSource)
                {
                    return decimalSource.AsDecimal();
                }
                else
                {
                    return sourceObject?.Value;
                }
            }
            else if (memberType == typeof(int) || memberType == typeof(int?)
                || memberType == typeof(short) || memberType == typeof(short?)
                || memberType == typeof(long) || memberType == typeof(long?)
                || memberType == typeof(sbyte) || memberType == typeof(sbyte?))
            {
                if (sourceObject is TpsDecimal decimalSource)
                {
                    decimal sourceValue = decimalSource.AsDecimal();
                    Type destType = Nullable.GetUnderlyingType(memberType) ?? memberType;

                    // Convert will round to the nearest whole value, so we floor/ceiling the value to emulate behavior of an explicit cast.
                    decimal value = sourceValue < 0 ? Math.Ceiling(sourceValue) : Math.Floor(sourceValue);

                    return Convert.ChangeType(value, destType);
                }
                else
                {
                    return sourceObject?.Value;
                }
            }
            else if (memberType == typeof(uint) || memberType == typeof(uint?)
                || memberType == typeof(ushort) || memberType == typeof(ushort?)
                || memberType == typeof(ulong) || memberType == typeof(ulong?)
                || memberType == typeof(byte) || memberType == typeof(byte?))
            {
                if (sourceObject is TpsDecimal decimalSource)
                {
                    decimal sourceValue = decimalSource.AsDecimal();
                    Type destType = Nullable.GetUnderlyingType(memberType) ?? memberType;

                    // Convert will round to the nearest whole value, so we floor the value to emulate the behavior of an explicit cast.
                    // If the value is negative, we coerce it to zero.
                    decimal value = sourceValue < 0 ? 0 : Math.Floor(sourceValue);

                    return Convert.ChangeType(value, destType);
                }
                else
                {
                    return sourceObject?.Value;
                }
            }
            else
            {
                return sourceObject?.Value;
            }
        }

        private object CoerceFallback(object value) => value is null ? FallbackValue : value;
    }
}
