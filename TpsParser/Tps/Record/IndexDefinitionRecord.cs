using System;
using TpsParser.Binary;

namespace TpsParser.Tps.Record
{
    public sealed class IndexDefinitionRecord
    {
        private string ExternalFile { get; }
        public string Name { get; }
        private int Flags { get; }
        public int FieldsInKey { get; }
        private int[] KeyField { get; }
        private int[] KeyFieldFlag { get; }

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

        public override string ToString() =>
            $"IndexDefinition({ExternalFile},{Name},{Flags},{FieldsInKey})";
    }
}
