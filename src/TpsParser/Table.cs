using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TpsParser
{
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

        /// <summary>
        /// Deserializes the table to a collection of the given type.
        /// </summary>
        /// <typeparam name="T">The type to represent a deserialized row.</typeparam>
        /// <returns></returns>
        public IEnumerable<T> Deserialize<T>() where T : class, new()
        {
            var targetClass = typeof(T);

            var targetObjects = Rows.Select(r => r.Deserialize<T>());

            return targetObjects;
        }

        /// <summary>
        /// Deserializes the table to a collection of the given type.
        /// </summary>
        /// <typeparam name="T">The type to represent a deserialized row.</typeparam>
        /// <returns></returns>
        public Task<IEnumerable<T>> DeserializeAsync<T>(CancellationToken ct = default) where T : class, new()
        {
            var targetClass = typeof(T);

            var targetObjects = Rows.Select(r => r.DeserializeAsync<T>(ct).Result);

            return Task.FromResult(targetObjects);
        }
    }
}
