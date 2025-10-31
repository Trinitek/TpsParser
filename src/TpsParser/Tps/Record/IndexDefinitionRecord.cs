using System;
using TpsParser.Binary;

namespace TpsParser.Tps.Record
{
    /// <summary>
    /// Represents the schema for a particular index.
    /// </summary>
    public interface IIndexDefinitionRecord
    {
        /// <summary>
        /// Gets the name of the index.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the number of fields tracked by the index.
        /// </summary>
        int FieldsInKey { get; }
    }

    /// <summary>
    /// Represents the schema for a particular index.
    /// </summary>
    internal sealed class IndexDefinitionRecord : IIndexDefinitionRecord
    {
        private string ExternalFile { get; }
        private int[] KeyField { get; }
        private int[] KeyFieldFlag { get; }
        private int Flags { get; }

        public string Name { get; }

        public int FieldsInKey { get; }

        public IndexDefinitionRecord(TpsRandomAccess rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            ExternalFile = rx.ReadZeroTerminatedString();

            if (ExternalFile.Length == 0)
            {
                int read = rx.ReadByte();

                if (read != 0x01)
                {
                    throw new ArgumentException($"Bad index definition: missing 0x01 after zero string ({read:X2})");
                }
            }

            Name = rx.ReadZeroTerminatedString();
            Flags = rx.ReadByte();
            FieldsInKey = rx.ReadShortLE();

            KeyField = new int[FieldsInKey];
            KeyFieldFlag = new int[FieldsInKey];

            for (int i = 0; i < FieldsInKey; i++)
            {
                KeyField[i] = rx.ReadShortLE();
                KeyFieldFlag[i] = rx.ReadShortLE();
            }
        }

        public override string ToString() =>
            $"IndexDefinition({ExternalFile},{Name},{Flags},{FieldsInKey})";
    }
}
