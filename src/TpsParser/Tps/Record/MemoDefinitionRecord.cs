﻿using System;
using System.Linq;

namespace TpsParser.Tps.Record
{
    /// <summary>
    /// Represents the schema for a particular MEMO or BLOB field.
    /// </summary>
    public interface IMemoDefinitionRecord
    {
        /// <summary>
        /// <para>
        /// Gets the fully qualified name of the field with the table prefix, e.g. "INV:INVOICENO".
        /// Use <see cref="Name"/> for only the field name.
        /// </para>
        /// <para>
        /// If the table was not defined with a prefix in Clarion, then it will be absent.
        /// When present, it is rarely the same as the table name, if the table has a name at all.
        /// </para>
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// <para>
        /// Gets the name of the field without the table prefix, e.g. "INVOICENO".
        /// Use <see cref="FullName"/> for the fully qualified field name.
        /// </para>
        /// </summary>
        string Name { get; }

        int Flags { get; }

        /// <summary>
        /// Returns true if the record contains MEMO data.
        /// </summary>
        bool IsMemo { get; }

        /// <summary>
        /// Returns true if the record contains BLOB data.
        /// </summary>
        bool IsBlob { get; }
    }

    internal sealed class MemoDefinitionRecord : IMemoDefinitionRecord
    {
        private string ExternalFile { get; }

        public string FullName { get; }

        public string Name => FullName.Split(':').Last();

        private int Length { get; }

        public int Flags { get; }

        public bool IsMemo => (Flags & 0x04) == 0;

        public bool IsBlob => !IsMemo;

        public MemoDefinitionRecord(TpsReader rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            ExternalFile = rx.ZeroTerminatedString();

            if (ExternalFile.Length == 0)
            {
                byte memoMarker = rx.ReadByte();

                if (memoMarker != 1)
                {
                    throw new ArgumentException($"Bad memo definition: missing 0x01 after zero string. Was 0x{memoMarker:x2}.");
                }
            }

            FullName = rx.ZeroTerminatedString();
            Length = rx.ReadShortLE();
            Flags = rx.ReadShortLE();
        }

        public override string ToString()
        {
            return $"MemoDefinition({ExternalFile},{FullName},{Length},{Flags})";
        }
    }
}
