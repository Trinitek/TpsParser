using System;
using System.Collections.Generic;

namespace TpsParser;

/// <summary>
/// Provides a high level representation of a table within a TopSpeed file.
/// </summary>
public sealed class Table
{
    /// <summary>
    /// Gets the name of the table.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets an unsorted collection of rows that belong to this table.
    /// </summary>
    public IEnumerable<Row> Rows { get; }

    /// <summary>
    /// Instantiates a new table.
    /// </summary>
    /// <param name="name">The name of the table.</param>
    /// <param name="rows">The rows that belong to the table.</param>
    public Table(string name, IEnumerable<Row> rows)
    {
        Name = name;
        Rows = rows ?? throw new ArgumentNullException(nameof(rows));
    }
}
