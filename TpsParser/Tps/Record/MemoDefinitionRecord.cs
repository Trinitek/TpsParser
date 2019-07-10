using System;
using TpsParser.Binary;

namespace TpsParser.Tps.Record
{
    public sealed class MemoDefinitionRecord
    {
        private string ExternalFile { get; }

        /// <summary>
        /// Gets the name of the memo field.
        /// </summary>
        public string Name { get; }

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

            Name = rx.ZeroTerminatedString();
            Length = rx.ShortLE();
            Flags = rx.ShortLE();
        }

        public override string ToString()
        {
            return $"MemoDefinition({ExternalFile},{Name},{Length},{Flags})";
        }
    }
}
