using System;
using System.Linq;
using TpsParser.Binary;

namespace TpsParser.Tps.Record
{
    public sealed class MemoDefinitionRecord
    {
        private string ExternalFile { get; }

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
        public string FullName { get; }

        /// <summary>
        /// <para>
        /// Gets the name of the field without the table prefix, e.g. "INVOICENO".
        /// Use <see cref="FullName"/> for the fully qualified field name.
        /// </para>
        /// </summary>
        public string Name => FullName.Split(':').Last();

        private int Length { get; }
        public int Flags { get; }

        public bool IsMemo => (Flags & 0x04) == 0;
        public bool IsBlob => !IsMemo;

        public MemoDefinitionRecord(RandomAccess rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            ExternalFile = rx.ZeroTerminatedString();

            if (ExternalFile.Length == 0)
            {
                byte memoMarker = rx.Byte();

                if (memoMarker != 1)
                {
                    throw new ArgumentException($"Bad memo definition: missing 0x01 after zero string. Was 0x{memoMarker:x2}.");
                }
            }

            FullName = rx.ZeroTerminatedString();
            Length = rx.ShortLE();
            Flags = rx.ShortLE();
        }

        public override string ToString()
        {
            return $"MemoDefinition({ExternalFile},{FullName},{Length},{Flags})";
        }
    }
}
