using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        /// This contains data fields as well as any associated MEMOs or BLOBs.
        /// </para>
        /// </summary>
        public IReadOnlyDictionary<string, ITpsObject> Values { get; }

        private DeserializerContext DeserializerContext { get; }

        /// <summary>
        /// Instantiates a new row.
        /// </summary>
        /// <param name="deserializerContext">The deserializer context.</param>
        /// <param name="recordNumber">The record number of the row.</param>
        /// <param name="values">The values in the row, keyed by their column names.</param>
        internal Row(DeserializerContext deserializerContext, int recordNumber, IReadOnlyDictionary<string, ITpsObject> values)
        {
            DeserializerContext = deserializerContext ?? throw new ArgumentNullException(nameof(deserializerContext));
            Id = recordNumber;
            Values = values ?? throw new ArgumentNullException(nameof(values));
        }

        /// <summary>
        /// Gets the field value, memo, or blob associated with the given column name.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public ITpsObject GetValue(string column) =>  Values[column];

        /// <summary>
        /// Gets the field, memo, or blob value associated with the given column name.
        /// If the column is not found and <paramref name="isRequired"/> is false, null is returned.
        /// </summary>
        /// <param name="column">The case insensitive name of the column.</param>
        /// <param name="isRequired">Indicates that the requested field must be present, or an exception is thrown.</param>
        /// <returns></returns>
        public ITpsObject GetValueCaseInsensitive(string column, bool isRequired)
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

        /// <summary>
        /// Deserializes the row into the given type. Members on the type should be marked with the appropriate attributes.
        /// </summary>
        /// <typeparam name="T">The type to represent the deserialized row.</typeparam>
        /// <returns></returns>
        public Task<T> DeserializeAsync<T>(CancellationToken ct = default) where T : class, new()
        {
            var targetObject = new T();

            SetMembers(targetObject);

            ct.ThrowIfCancellationRequested();

            return Task.FromResult(targetObject);
        }

        private void SetMembers<T>(T targetObject) where T : class
        {
            var members = DeserializerContext.GetModelMembers(targetObject);

            foreach (var member in members)
            {
                if (member.IsRecordNumber)
                {
                    member.SetMember(targetObject, new TpsLong(Id));
                }
                else
                {
                    string tpsFieldName = member.FieldAttribute.FieldName;
                    ITpsObject tpsFieldValue = GetRowValue(tpsFieldName, member.FieldAttribute.IsRequired);
                    //object clrValue = member.FieldAttribute.InterpretValue(member.MemberInfo, tpsFieldValue);

                    try
                    {
                        //member.SetMember(targetObject, clrValue);
                        member.SetMember(targetObject, tpsFieldValue);
                    }
                    catch (Exception ex) when (!(ex is TpsParserException))
                    {
                        throw new TpsParserException(
                            //$"Cannot set member [{member}] to value '{clrValue}' of type '{clrValue?.GetType()}' " +
                            $"Cannot set member [{member}] to value '{tpsFieldValue}' of type '{tpsFieldValue?.GetType().Name}'. " +
                            //$"(Source value '{tpsFieldValue}' of {nameof(TpsObject)} type '{tpsFieldValue.GetType().Name}'). " +
                            $"See the inner exception for details.", ex);
                    }
                }
            }
        }

        private ITpsObject GetRowValue(string fieldName, bool isRequired)
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
    }
}
