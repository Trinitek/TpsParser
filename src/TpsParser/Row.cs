using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TpsParser.Tps.Type;

namespace TpsParser
{
    /// <summary>
    /// Provides a high level representation of a row within a TopSpeed file.
    /// </summary>
    public class Row
    {
        /// <summary>
        /// Gets the record number.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// <para>
        /// Gets the field values that belong to the record, where each <see cref="TpsObject"/> is associated with the name of its column.
        /// </para>
        /// <para>
        /// This contains data fields as well as any associated memos or blobs.
        /// </para>
        /// </summary>
        public IReadOnlyDictionary<string, TpsObject> Values { get; }

        /// <summary>
        /// Instantiates a new row.
        /// </summary>
        /// <param name="recordNumber">The record number of the row.</param>
        /// <param name="values">The values in the row, keyed by their column names.</param>
        public Row(int recordNumber, IReadOnlyDictionary<string, TpsObject> values)
        {
            Id = recordNumber;
            Values = values ?? throw new ArgumentNullException(nameof(values));
        }

        /// <summary>
        /// Gets the field value, memo, or blob associated with the given column name.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public TpsObject GetValue(string column) =>  Values[column];

        /// <summary>
        /// Gets the field, memo, or blob value associated with the given column name.
        /// If the column is not found and <paramref name="isRequired"/> is false, null is returned.
        /// </summary>
        /// <param name="column">The case insensitive name of the column.</param>
        /// <param name="isRequired">Indicates that the requested field must be present, or an exception is thrown.</param>
        /// <returns></returns>
        public TpsObject GetValueCaseInsensitive(string column, bool isRequired)
        {
            var matchingKey = Values.Keys.FirstOrDefault(k => k.Equals(column, StringComparison.OrdinalIgnoreCase));

            if (matchingKey is null)
            {
                if (isRequired)
                {
                    var sb = new StringBuilder();

                    foreach (var key in Values.Keys)
                    {
                        sb.Append($"{key}, ");
                    }

                    var keyList = sb.ToString();

                    if (Values.Keys.Any())
                    {
                        keyList = keyList.Substring(0, keyList.Length - 2); // Trim trailing space and comma
                    }

                    throw new ArgumentException($"Could not find column by case insensitive name '{column}'. Available columns are [{keyList}].");
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return Values[matchingKey];
            }
        }

        /// <summary>
        /// Deserializes the row into the given type. Members on the type should be marked with the appropriate attributes.
        /// </summary>
        /// <typeparam name="T">The type to represent the deserialized row.</typeparam>
        /// <returns></returns>
        public T Deserialize<T>() where T : class, new()
        {
            var targetObject = new T();

            SetMembers(targetObject);

            return targetObject;
        }

        private void SetMembers<T>(T targetObject)
        {
            var members = typeof(T).GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var member in members)
            {
                var tpsFieldAttr = member.GetCustomAttribute<TpsFieldAttribute>();
                var tpsRecordNumberAttr = member.GetCustomAttribute<TpsRecordNumberAttribute>();

                if (tpsFieldAttr != null && tpsRecordNumberAttr != null)
                {
                    throw new TpsParserException($"Members cannot be marked with both {nameof(TpsFieldAttribute)} and {nameof(TpsRecordNumberAttribute)}. Property name '{member.Name}'.");
                }

                if (tpsFieldAttr != null)
                {
                    string tpsFieldName = tpsFieldAttr.FieldName;
                    TpsObject tpsFieldValue = GetRowValue(tpsFieldName, tpsFieldAttr.IsRequired);
                    object convertedTpsValue = ConvertValue(GetMemberType(member), tpsFieldValue);
                    object stringyTpsValue = ApplyStringConversion(member, tpsFieldAttr, convertedTpsValue);
                    object tpsValue = CoerceNullValue(stringyTpsValue, tpsFieldAttr.FallbackValue);

                    SetMember(member, targetObject, tpsValue);
                }
                if (tpsRecordNumberAttr != null)
                {
                    SetMember(member, targetObject, Id);
                }
            }
        }

        private static object ApplyStringConversion(MemberInfo member, TpsFieldAttribute tpsFieldAttr, object tpsValue)
        {
            var memberType = GetMemberType(member);

            if (tpsFieldAttr is TpsFieldStringAttribute tpsFieldStringAttr && tpsValue != null)
            {
                if (memberType != typeof(string))
                {
                    throw new TpsParserException($"{nameof(TpsFieldStringAttribute)} is only valid on members of type {typeof(string)} ({member}).");
                }

                if (tpsValue is string tpsStringValue)
                {
                    return tpsStringValue?.TrimEnd() ?? tpsValue;
                }
                else
                {
                    return InvokeToString(member, tpsValue, tpsFieldStringAttr);
                }

            }
            else
            {
                return tpsValue;
            }
        }

        private static string InvokeToString(MemberInfo member, object tpsValue, TpsFieldStringAttribute tpsFieldStringAttr)
        {
            try
            {
                if (tpsValue.GetType().GetMethod("ToString", new Type[] { typeof(string) }) is var miStringParam && miStringParam != null)
                {
                    return (string)miStringParam.Invoke(tpsValue, new string[] { tpsFieldStringAttr.StringFormat });
                }
                else if (tpsValue.GetType().GetMethod("ToString", Type.EmptyTypes) is var miNoParams && miNoParams != null)
                {
                    return (string)miNoParams.Invoke(tpsValue, null);
                }
                else
                {
                    throw new TpsParserException("No suitable ToString method was found on the object. This is probably a bug in the library.");
                }
            }
            catch (Exception ex)
            {
                throw new TpsParserException($"Unable to apply string conversion on the given member. See the inner exception for details. ({member})", ex);
            }
        }

        private static void SetMember(MemberInfo member, object target, object value)
        {
            if (member is PropertyInfo prop)
            {
                if (!prop.CanWrite)
                {
                    throw new TpsParserException($"The property '{member.Name}' must have a setter.");
                }

                prop.SetValue(target, value);
            }
            else if (member is FieldInfo field)
            {
                field.SetValue(target, value);
            }
        }

        private TpsObject GetRowValue(string fieldName, bool isRequired)
        {
            try
            {
                return GetValueCaseInsensitive(fieldName, isRequired);
            }
            catch (Exception ex)
            {
                throw new TpsParserException("Unable to deserialize field into class member. See the inner exception for details.", ex);
            }
        }

        private static Type GetMemberType(MemberInfo info)
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

        /// <summary>
        /// Performs conversion on the source <see cref="TpsObject"/> to another object type compatible with the target
        /// member's type where a conversion exists and returns the value. If a conversion does not exist or is not necessary,
        /// the value of the original object is returned. (Ex.: <see cref="DateTime"/> members can accept a value from
        /// <see cref="TpsLong"/> in addition to its native <see cref="TpsDate"/>.
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="sourceObject"></param>
        /// <returns></returns>
        private static object ConvertValue(Type targetType, TpsObject sourceObject)
        {
            if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
            {
                if (sourceObject is TpsLong longSource)
                {
                    return new TpsDate(longSource.AsDate()).Value;
                }
                else
                {
                    return sourceObject?.Value;
                }
            }
            else if (targetType == typeof(TimeSpan) || targetType == typeof(TimeSpan?))
            {
                if (sourceObject is TpsLong longSource)
                {
                    return new TpsTime(longSource.AsTime()).Value;
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

        private static object CoerceNullValue(object value, object fallback)
        {
            if (fallback != null)
            {
                return value ?? fallback;
            }
            else
            {
                return value;
            }
        }
    }
}
