using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TpsParser.Tps.Type;

namespace TpsParser
{
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
    }
}
