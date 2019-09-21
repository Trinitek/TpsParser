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
    public sealed class IndexDefinitionRecord : IIndexDefinitionRecord
    {
        private string ExternalFile { get; }
        private int[] KeyField { get; }
        private int[] KeyFieldFlag { get; }
        private int Flags { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public int FieldsInKey { get; }

        public IndexDefinitionRecord(RandomAccess rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            ExternalFile = rx.ZeroTerminatedString();

            if (ExternalFile.Length == 0)
            {
                int read = rx.Byte();

                if (read != 0x01)
                {
                    throw new ArgumentException($"Bad index definition: missing 0x01 after zero string ({read:X2})");
                }
            }

            Name = rx.ZeroTerminatedString();
            Flags = rx.Byte();
            FieldsInKey = rx.ShortLE();

            KeyField = new int[FieldsInKey];
            KeyFieldFlag = new int[FieldsInKey];

            for (int i = 0; i < FieldsInKey; i++)
            {
                KeyField[i] = rx.ShortLE();
                KeyFieldFlag[i] = rx.ShortLE();
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override string ToString() =>
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
            $"IndexDefinition({ExternalFile},{Name},{Flags},{FieldsInKey})";
    }
}
