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

        /// <summary>
        /// Gets the type of the member on which the attribute is marked.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        protected static Type GetMemberType(MemberInfo info)
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

            object interpretedValue;

            try
            {
                interpretedValue = InterpretValuePrivate(memberType, sourceObject);
            }
            catch (Exception ex)
            {
                throw new TpsParserException(
                    $"Could not interpret value of type {sourceObject.GetType()} as {memberType}. " +
                    $"See the inner exception for details.", ex);
            }

            return CoerceFallback(interpretedValue);
        }
        
        private static object InterpretValuePrivate(Type memberType, TpsObject sourceObject)
        {
            if (memberType == typeof(DateTime))
            {
                return sourceObject.ToDateTime().Value ?? default;
            }
            else if (memberType == typeof(DateTime?))
            {
                return sourceObject.ToDateTime().Value;
            }
            else if (memberType == typeof(TimeSpan))
            {
                return sourceObject.ToTimeSpan().Value;
            }
            else if (memberType == typeof(TimeSpan?))
            {
                return sourceObject.ToTimeSpan().AsNullable();
            }
            else if (memberType == typeof(bool))
            {
                return sourceObject.ToBoolean().Value;
            }
            else if (memberType == typeof(bool?))
            {
                return sourceObject.ToBoolean().AsNullable();
            }
            else if (memberType == typeof(string))
            {
                return sourceObject.ToString();
            }
            else if (memberType == typeof(decimal))
            {
                return sourceObject.ToDecimal().Value;
            }
            else if (memberType == typeof(decimal?))
            {
                return sourceObject.ToDecimal().AsNullable();
            }
            else if (memberType == typeof(int))
            {
                return sourceObject.ToInt32().Value;
            }
            else if (memberType == typeof(int?))
            {
                return sourceObject.ToInt32().AsNullable();
            }
            else if (memberType == typeof(short))
            {
                return sourceObject.ToInt16().Value;
            }
            else if (memberType == typeof(short?))
            {
                return sourceObject.ToInt16().AsNullable();
            }
            else if (memberType == typeof(long))
            {
                return sourceObject.ToInt64().Value;
            }
            else if (memberType == typeof(long?))
            {
                return sourceObject.ToInt64().AsNullable();
            }
            else if (memberType == typeof(sbyte))
            {
                return sourceObject.ToSByte().Value;
            }
            else if (memberType == typeof(sbyte?))
            {
                return sourceObject.ToSByte().AsNullable();
            }
            else if (memberType == typeof(uint))
            {
                return sourceObject.ToUInt32().Value;
            }
            else if (memberType == typeof(uint?))
            {
                return sourceObject.ToUInt32().AsNullable();
            }
            else if (memberType == typeof(ushort))
            {
                return sourceObject.ToUInt16().Value;
            }
            else if (memberType == typeof(ushort?))
            {
                return sourceObject.ToUInt16().AsNullable();
            }
            else if (memberType == typeof(ulong))
            {
                return sourceObject.ToUInt64().Value;
            }
            else if (memberType == typeof(ulong?))
            {
                return sourceObject.ToUInt64().AsNullable();
            }
            else if (memberType == typeof(byte))
            {
                return sourceObject.ToByte().Value;
            }
            else if (memberType == typeof(byte?))
            {
                return sourceObject.ToByte().AsNullable();
            }
            else
            {
                return sourceObject?.Value;
            }
        }

        private object CoerceFallback(object value) => value is null ? FallbackValue : value;
    }
}
