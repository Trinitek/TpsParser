using System;
using System.Collections.Generic;

namespace TpsParser
{
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

        public Table(string name, IEnumerable<Row> rows)
        {
            Name = name;
            Rows = rows ?? throw new ArgumentNullException(nameof(rows));
        }
    }
}
