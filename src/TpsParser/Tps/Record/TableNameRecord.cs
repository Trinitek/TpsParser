﻿using System;
using TpsParser.Tps.Header;

namespace TpsParser.Tps.Record
{
    /// <summary>
    /// Represents a file structure that contains the table number for an associated name in <see cref="TableNameHeader"/>.
    /// </summary>
    public sealed class TableNameRecord
    {
        /// <summary>
        /// Gets the header of the record.
        /// </summary>
        public ITableNameHeader Header { get; }

        /// <summary>
        /// Gets the associated table number.
        /// </summary>
        public int TableNumber { get; }

        /// <summary>
        /// Instantiates a new table name record.
        /// </summary>
        /// <param name="record"></param>
        public TableNameRecord(TpsRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            Header = (ITableNameHeader)record.Header;
            TableNumber = record.Data.ReadLongBE();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"TableRecord({Header.Name},{TableNumber})";
        }
    }
}
