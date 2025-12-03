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
    public int RecordNumber { get; }

    /// <summary>
    /// <para>
    /// Gets the field values that belong to the record, where each <see cref="IClaObject"/> is associated with the name of its column.
    /// </para>
    /// </summary>
    public IReadOnlyDictionary<string, IClaObject> Values { get; }

    /// <summary>
    /// Gets the <c>MEMO</c> or <c>BLOB</c> objects that belong to the record, where each <see cref="ITpsMemo"/> is associated with the name of its column.
    /// </summary>
    public IReadOnlyDictionary<string, ITpsMemo> Memos { get; }

    /// <summary>
    /// Instantiates a new row.
    /// </summary>
    /// <param name="recordNumber">The record number of the row.</param>
    /// <param name="values">The values in the row, keyed by their column names.</param>
    /// <param name="memos">The <c>MEMO</c>s in the row, keyed by their column names.</param>
    public Row(int recordNumber, IReadOnlyDictionary<string, IClaObject> values, IReadOnlyDictionary<string, ITpsMemo> memos)
    {
        RecordNumber = recordNumber;
        Values = values ?? throw new ArgumentNullException(nameof(values));
        Memos = memos ?? throw new ArgumentNullException(nameof(memos));
    }

    /// <summary>
    /// Gets the field, value associated with the given column name.
    /// If the column is not found and <paramref name="isRequired"/> is false, null is returned.
    /// </summary>
    /// <param name="column">The case insensitive name of the column.</param>
    /// <param name="isRequired">Indicates that the requested field must be present, or an exception is thrown.</param>
    /// <returns></returns>
    public IClaObject? GetFieldValueCaseInsensitive(string column, bool isRequired)
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
    /// Gets the <c>MEMO</c> or <c>BLOB</c> associated with the given column name.
    /// If the column is not found and <paramref name="isRequired"/> is false, null is returned.
    /// </summary>
    /// <param name="column"></param>
    /// <param name="isRequired"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public ITpsMemo? GetMemoCaseInsensitive(string column, bool isRequired)
    {
        var matchingKey = Memos.Keys.FirstOrDefault(k => k.Equals(column, StringComparison.OrdinalIgnoreCase));
        if (matchingKey is null)
        {
            if (isRequired)
            {
                var sb = new StringBuilder();
                foreach (var key in Memos.Keys)
                {
                    sb.Append($"{key}, ");
                }

                var keyList = sb.ToString();

                if (Memos.Keys.Any())
                {
                    keyList = keyList.Substring(0, keyList.Length - 2); // Trim trailing space and comma
                }

                throw new ArgumentException($"Could not find MEMO column by case insensitive name '{column}'. Available MEMO columns are [{keyList}].");
            }
            else
            {
                return null;
            }
        }
        else
        {
            return Memos[matchingKey];
        }
    }
}
