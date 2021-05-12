using System;

namespace TpsParser.Tps.Header
{
    /// <summary>
    /// Encapsulates information about a table.
    /// </summary>
    public sealed class TableDefinitionHeader : HeaderBase
    {
        public int Block { get; }

        /// <summary>
        /// Instantiates a new header.
        /// </summary>
        /// <param name="tableNumber"></param>
        /// <param name="kind"></param>
        /// <param name="block"></param>
        public TableDefinitionHeader(int tableNumber, HeaderKind kind, int block)
            : base(tableNumber, kind)
        {
            AssertIsType(HeaderKind.TableDefinition);

            Block = block;
        }

        /// <inheritdoc/>
        public override string ToString() =>
            $"TableDef({TableNumber}, {Block})";

        /// <summary>
        /// Creates a new <see cref="TableDefinitionHeader"/> from the given reader.
        /// </summary>
        /// <param name="rx"></param>
        /// <returns></returns>
        public static TableDefinitionHeader Read(TpsReader rx)
        {
            if (rx is null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            return new TableDefinitionHeader(
                tableNumber: rx.ReadLongBE(),
                kind: (HeaderKind)rx.ReadByte(),
                block: rx.ReadShortLE());
        }
    }
}
