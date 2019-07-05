using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TpsParser.Binary;

namespace TpsParser.Tps.KeyRecovery
{
    public sealed class Block : IComparable<Block>, IEquatable<Block>
    {
        public int Offset { get; }

        public IReadOnlyList<int> Values => _values;
        private int[] _values;

        public bool IsEncrypted { get; }

        public Block(int offset, IReadOnlyList<int> values, bool isEncrypted)
        {
            Offset = offset;
            _values = values?.ToArray() ?? throw new ArgumentNullException(nameof(values));
            IsEncrypted = isEncrypted;
        }

        public Block(Block block)
            : this(
                  offset: block.Offset,
                  values: block.Values,
                  isEncrypted: block.IsEncrypted)
        { }

        public Block(int offset, Block block)
            : this(
                  offset: offset,
                  values: block.Values,
                  isEncrypted: block.IsEncrypted)
        { }

        public Block(RandomAccess rx, bool isEncrypted)
            : this(
                  offset: rx.Position,
                  values: rx.LongArrayLE(16),
                  isEncrypted: isEncrypted)
        { }

        /// <summary>
        /// Apply a partial encryption or decryption.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="va"></param>
        /// <param name="vb"></param>
        /// <returns></returns>
        public Block Apply(int a, int b, int va, int vb)
        {
            var target = new Block(this);
            target._values[a] = va;
            target._values[b] = vb;
            return target;
        }

        public bool ValueEquals(Block block) => Values.SequenceEqual(block.Values);

        public void Write(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.Write(Offset);
            writer.Write(IsEncrypted);
            writer.Write(Values.Count);
            
            foreach (int value in Values)
            {
                writer.Write(value);
            }
        }

        public static void Read(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            int offset = reader.ReadInt32();
            bool isEncrypted = reader.ReadBoolean();
            int valueLength = reader.ReadInt32();

            int[] values = new int[valueLength];

            for (int i = 0; i < valueLength; i++)
            {
                values[i] = reader.ReadInt32();
            }
        }

        public int CompareTo(Block other)
        {
            int d = Offset - other.Offset;

            if (d == 0)
            {
                d = (IsEncrypted ? 1 : 0) - (other.IsEncrypted ? 1 : 0);

                if (d == 0)
                {
                    for (int i = 0; i < Values.Count; i++)
                    {
                        d = Values[i] - other.Values[i];

                        if (d != 0)
                        {
                            return d;
                        }
                    }
                }
            }

            return d;
        }

        public bool Equals(Block other)
        {
            if (other is null)
            {
                return false;
            }
            else
            {
                return Offset == other.Offset
                    && IsEncrypted == other.IsEncrypted
                    && Values.SequenceEqual(other.Values);
            }
        }

        public override bool Equals(object obj) => obj is Block b ? Equals(b) : false;

        public override int GetHashCode()
        {
            var hashCode = 1824363456;
            hashCode = hashCode * -1521134295 + Offset.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<IReadOnlyList<int>>.Default.GetHashCode(Values);
            hashCode = hashCode * -1521134295 + IsEncrypted.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Block left, Block right)
        {
            return EqualityComparer<Block>.Default.Equals(left, right);
        }

        public static bool operator !=(Block left, Block right)
        {
            return !(left == right);
        }
    }
}
