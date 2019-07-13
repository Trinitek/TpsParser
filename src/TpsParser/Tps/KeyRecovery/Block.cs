using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TpsParser.Binary;

namespace TpsParser.Tps.KeyRecovery
{
    public sealed class Block : IComparable<Block>, IEquatable<Block>
    {
        /// <summary>
        /// Gets the offset in the file at which the block resides.
        /// </summary>
        public int Offset { get; }

        /// <summary>
        /// Gets the integer values that compose the block.
        /// </summary>
        public IReadOnlyList<int> Values => _values;
        private readonly int[] _values;

        /// <summary>
        /// Returns true if the block is encrypted.
        /// </summary>
        public bool IsEncrypted { get; }

        /// <summary>
        /// Instantiates a new block.
        /// </summary>
        /// <param name="offset">The offset at which the block resides.</param>
        /// <param name="values">The integer values that compose the block.</param>
        /// <param name="isEncrypted">The block's encryption status.</param>
        public Block(int offset, IEnumerable<int> values, bool isEncrypted)
        {
            Offset = offset;
            _values = values?.ToArray() ?? throw new ArgumentNullException(nameof(values));
            IsEncrypted = isEncrypted;
        }

        /// <summary>
        /// Instantiates a new block from an existing block. The given block's offset, values, and encryption status are copied.
        /// </summary>
        /// <param name="block">The block that is to be copied.</param>
        public Block(Block block)
            : this(
                  offset: block.Offset,
                  values: block.Values,
                  isEncrypted: block.IsEncrypted)
        { }

        /// <summary>
        /// Instantiates a new block from an existing block. The given block's values and encryption status are copied.
        /// </summary>
        /// <param name="offset">The offset at which the block resides.</param>
        /// <param name="block">The block that is to be copied.</param>
        public Block(int offset, Block block)
            : this(
                  offset: offset,
                  values: block.Values,
                  isEncrypted: block.IsEncrypted)
        { }

        /// <summary>
        /// Instantiates a new block from a data stream.
        /// </summary>
        /// <param name="rx">The data stream from which to construct the block.</param>
        /// <param name="isEncrypted">The block's encryption status.</param>
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

        internal bool ValueEquals(Block block) => Values.SequenceEqual(block.Values);

        /// <summary>
        /// Writes the block's offset, encryption status, and values to an outgoing stream.
        /// </summary>
        /// <param name="writer"></param>
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

        /// <summary>
        /// Creates a new block from an incoming stream.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Block Read(BinaryReader reader)
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

            return new Block(
                offset: offset,
                values: values,
                isEncrypted: isEncrypted);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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
            hashCode = hashCode * -1521134295 + IsEncrypted.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append($"{Offset:x8} ({(IsEncrypted ? 'e' : 'd')}) : ");

            foreach (int value in Values)
            {
                sb.Append($"{value:x8} ");
            }

            return sb.ToString();
        }

        public static bool operator ==(Block left, Block right)
        {
            return EqualityComparer<Block>.Default.Equals(left, right);
        }

        public static bool operator !=(Block left, Block right)
        {
            return !(left == right);
        }

        /// <summary>
        /// <para>
        /// Find blocks with the same (encrypted) content in the file.
        /// </para>
        /// <para>
        /// TPS uses EBC mode, so identical encrypted
        /// blocks will map to the same plaintext. This is useful because identical blocks are generally empty smace
        /// whose plaintext contents are known in advance (0xB0B0B0B0).
        /// </para>
        /// </summary>
        /// <param name="blocks"></param>
        /// <returns></returns>
        public static IReadOnlyDictionary<Block, IEnumerable<Block>> FindIdenticalBlocks(IEnumerable<Block> blocks) =>
            blocks.GroupBy(b => b, BlockValueEqualityComparer.Instance)
                .Where(g => g.Count() > 1)
                .ToDictionary(g => g.Key, g => g.Skip(1).AsEnumerable());

        /// <summary>
        /// Blocks consisting of the pattern 0xB0B0B0B0 are occasionally scattered around the file and seem to indicate empty space.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsB0Part(uint value) => value == 0xB0B0B0B0;

        /// <summary>
        /// <para>
        /// Gets the header index end block.
        /// </para>
        /// <para>
        /// This is one of the few blocks in the TPS format with predictable contents and is at a fixed location.
        /// Typically it is filled with identical values (the size of the file, less the size of the header and right shifted for 8 bits).
        /// </para>
        /// </summary>
        /// <param name="blocks"></param>
        /// <param name="isEncrypted"></param>
        /// <returns></returns>
        public static Block GetHeaderIndexEndBlock(IList<Block> blocks, bool isEncrypted)
        {
            if (isEncrypted)
            {
                var block = blocks[0x1C0 / 0x40];

                if (block.Offset != 0x1C0)
                {
                    throw new ArgumentException(nameof(isEncrypted));
                }
                else
                {
                    return block;
                }
            }
            else
            {
                int value = (blocks.Last().Offset + 0x100 - 0x200) >> 8;

                var values = Enumerable.Repeat(value, 16);

                var block = new Block(offset: 0x1C0, values: values, isEncrypted: isEncrypted);

                return block;
            }
        }

        /// <summary>
        /// <para>
        /// Generates a sequence block that is usually found at the end of the file.
        /// </para>
        /// <para>
        /// Most files have an area with incrementing bytes near the end of the file. The exact offset differs, but given the last 4 bytes
        /// the block can be reconstructed.
        /// </para>
        /// </summary>
        /// <param name="end"></param>
        /// <returns></returns>
        public static Block GenerateSequenceBlock(int end)
        {
            int start = (end >> 24) & 0xFF;

            var sequence = new byte[64];

            for (int i = sequence.Length - 1; i >= 0; i--)
            {
                sequence[i] = (byte)start--;
            }

            return new Block(offset: 0, values: new RandomAccess(sequence).LongArrayLE(16), isEncrypted: false);
        }

        /// <summary>
        /// Returns true when the given value is a byte sequence like 0x2A292827 or 0x0100FFFE.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsSequencePart(int value)
        {
            int a = value & 0xFF;
            int b = (value >> 8) & 0xFF;
            int c = (value >> 16) & 0xFF;
            int d = (value >> 24) & 0xFF;

            return ((d - c == 1) || (d - c == -255))
                && ((c - b == 1) || (c - b == -255))
                && ((b - a == 1) || (b - a == -255));
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
