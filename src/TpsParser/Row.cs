using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TpsParser.TypeModel;

namespace TpsParser;

/// <summary>
/// Provides a high level representation of a row within a TopSpeed file.
/// </summary>
public sealed class Row
{
    /// <summary>
    /// Gets the record number.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// <para>
    /// Gets the field values that belong to the record, where each <see cref="ClaObject"/> is associated with the name of its column.
    /// </para>
    /// <para>
    /// This contains data fields as well as any associated memos or blobs.
    /// </para>
    /// </summary>
    public IReadOnlyDictionary<string, IClaObject> Values { get; }

    /// <summary>
    /// Instantiates a new row.
    /// </summary>
    /// <param name="recordNumber">The record number of the row.</param>
    /// <param name="values">The values in the row, keyed by their column names.</param>
    public Row(int recordNumber, IReadOnlyDictionary<string, IClaObject> values)
    {
        Id = recordNumber;
        Values = values ?? throw new ArgumentNullException(nameof(values));
    }

    /// <summary>
    /// Gets the field value, memo, or blob associated with the given column name.
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public IClaObject GetValue(string column) => Values[column];

    /// <summary>
    /// Gets the field, memo, or blob value associated with the given column name.
    /// If the column is not found and <paramref name="isRequired"/> is false, null is returned.
    /// </summary>
    /// <param name="column">The case insensitive name of the column.</param>
    /// <param name="isRequired">Indicates that the requested field must be present, or an exception is thrown.</param>
    /// <returns></returns>
    public IClaObject? GetValueCaseInsensitive(string column, bool isRequired)
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
