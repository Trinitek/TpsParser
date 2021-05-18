using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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

        internal virtual object InterpretValue(Type memberType, TpsObject sourceObject)
        {
            if (memberType is null)
            {
                throw new ArgumentNullException(nameof(memberType));
            }

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

        internal static Dictionary<Type, Expression<Func<TpsObject, object>>> ValueInterpreters { get; } =
            new Dictionary<Type, Expression<Func<TpsObject, object>>>
            {
                [typeof(DateTime)] = x => x.ToDateTime().Value ?? default,
                [typeof(DateTime?)] = x => x.ToDateTime().Value,
                [typeof(TimeSpan)] = x => x.ToTimeSpan().Value,
                [typeof(TimeSpan?)] = x => x.ToTimeSpan().AsNullable(),
                [typeof(bool)] = x => x.ToBoolean().Value,
                [typeof(bool?)] = x => x.ToBoolean().AsNullable(),
                [typeof(string)] = x => x.ToString(),
                [typeof(decimal)] = x => x.ToDecimal().Value,
                [typeof(decimal?)] = x => x.ToDecimal().AsNullable(),
                [typeof(int)] = x => x.ToInt32().Value,
                [typeof(int?)] = x => x.ToInt32().AsNullable(),
                [typeof(short)] = x => x.ToInt16().Value,
                [typeof(short?)] = x => x.ToInt16().AsNullable(),
                [typeof(long)] = x => x.ToInt64().Value,
                [typeof(long?)] = x => x.ToInt64().AsNullable(),
                [typeof(sbyte)] = x => x.ToSByte().Value,
                [typeof(sbyte?)] = x => x.ToSByte().AsNullable(),
                [typeof(uint)] = x => x.ToUInt32().Value,
                [typeof(uint?)] = x => x.ToUInt32().AsNullable(),
                [typeof(ushort)] = x => x.ToUInt16().Value,
                [typeof(ushort?)] = x => x.ToUInt16().AsNullable(),
                [typeof(ulong)] = x => x.ToUInt64().Value,
                [typeof(ulong?)] = x => x.ToUInt64().AsNullable(),
                [typeof(byte)] = x => x.ToByte().Value,
                [typeof(byte?)] = x => x.ToByte().AsNullable(),
            };
        
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
