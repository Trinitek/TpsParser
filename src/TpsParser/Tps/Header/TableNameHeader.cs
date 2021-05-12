using System;

namespace TpsParser.Tps.Header
{
    /// <summary>
    /// Represents a file structure that contains the name of a table.
    /// </summary>
    public sealed class TableNameHeader : HeaderBase
    {
        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Instantiates a new header that describes the name of the table.
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="name"></param>
        public TableNameHeader(HeaderKind kind, string name)
            : base(default, kind)
        {
            AssertIsType(HeaderKind.TableName);

            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <inheritdoc/>
        public override string ToString() =>
            $"TableName({Name})";

        /// <summary>
        /// Creates a new <see cref="TableNameHeader"/> from the given reader.
        /// </summary>
        /// <param name="rx"></param>
        /// <returns></returns>
        public static TableNameHeader Read(TpsReader rx)
        {
            if (rx is null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            return new TableNameHeader(
                kind: (HeaderKind)rx.ReadByte(),
                name: rx.FixedLengthString(rx.Length - rx.Position));
        }
    }
}
