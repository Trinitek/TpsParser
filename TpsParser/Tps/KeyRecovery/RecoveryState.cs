using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TpsParser.Tps.KeyRecovery
{
    public sealed class RecoveryState : IComparable<RecoveryState>, IEquatable<RecoveryState>
    {
        public RecoveryState Parent { get; }

        public PartialKey PartialKey { get; }

        private Block EncryptedHeaderBlock { get; }
        private Block PlaintextHeaderBlock { get; }

        public IEnumerable<Block> B0Blocks { get; }
        public IEnumerable<Block> SequentialBlocks { get; }

        private RecoveryState(RecoveryState parent, PartialKey partialKey, IEnumerable<Block> b0Blocks, IEnumerable<Block> sequentialBlocks, Block encryptedHeaderBlock, Block plaintextHeaderBlock)
        {
            Parent = parent;
            PartialKey = partialKey ?? throw new ArgumentNullException(nameof(partialKey));
            B0Blocks = b0Blocks ?? throw new ArgumentNullException(nameof(b0Blocks));
            SequentialBlocks = sequentialBlocks ?? throw new ArgumentNullException(nameof(sequentialBlocks));
            EncryptedHeaderBlock = encryptedHeaderBlock ?? throw new ArgumentNullException(nameof(encryptedHeaderBlock));
            PlaintextHeaderBlock = plaintextHeaderBlock ?? throw new ArgumentNullException(nameof(plaintextHeaderBlock));
        }

        private RecoveryState(PartialKey partialKey, IEnumerable<Block> b0Blocks, IEnumerable<Block> sequentialBlocks, Block encryptedHeaderBlock, Block plaintextHeaderBlock)
            : this(
                  parent: null,
                  partialKey: partialKey,
                  b0Blocks: b0Blocks,
                  sequentialBlocks: sequentialBlocks,
                  encryptedHeaderBlock: encryptedHeaderBlock,
                  plaintextHeaderBlock: plaintextHeaderBlock)
        { }

        private RecoveryState(RecoveryState parent, PartialKey partialKey, Block partiallyDecryptedBlock)
            : this(
                  parent: parent,
                  partialKey: partialKey,
                  b0Blocks: parent.B0Blocks,
                  sequentialBlocks: parent.SequentialBlocks,
                  encryptedHeaderBlock: partiallyDecryptedBlock,
                  plaintextHeaderBlock: parent.PlaintextHeaderBlock)
        { }

        internal RecoveryState(RecoveryState parent, IEnumerable<Block> b0Blocks, IEnumerable<Block> sequentialBlocks)
            : this(
                  parent: parent,
                  partialKey: parent.PartialKey,
                  b0Blocks: b0Blocks,
                  sequentialBlocks: sequentialBlocks,
                  encryptedHeaderBlock: parent.EncryptedHeaderBlock,
                  plaintextHeaderBlock: parent.PlaintextHeaderBlock)
        { }

        public RecoveryState(Block encryptedHeaderBlock, Block plaintextHeaderBlock)
            : this(
                  parent: null,
                  partialKey: new PartialKey(),
                  b0Blocks: Enumerable.Empty<Block>(),
                  sequentialBlocks: Enumerable.Empty<Block>(),
                  encryptedHeaderBlock: encryptedHeaderBlock,
                  plaintextHeaderBlock: plaintextHeaderBlock)
        { }

        internal static RecoveryState CreateFromSequentialBlocks(RecoveryState parent, IEnumerable<Block> sequentialBlocks) =>
            new RecoveryState(parent, parent.B0Blocks, sequentialBlocks);

        internal static RecoveryState CreateFromB0Blocks(RecoveryState parent, IEnumerable<Block> b0blocks) =>
            new RecoveryState(parent, b0blocks, parent.SequentialBlocks);

        /// <summary>
        /// Scans the given key index to find a set of potential values that can decrypt the header block. Takes all 64 bits into consideration.
        /// </summary>
        /// <param name="keyIndex"></param>
        /// <returns></returns>
        public async Task<IEnumerable<RecoveryState>> IndexScan(int keyIndex, CancellationToken cancellationToken)
        {
            var results = await PartialKey.KeyIndexScan(keyIndex, EncryptedHeaderBlock, PlaintextHeaderBlock, cancellationToken)
                .ConfigureAwait(false);

            return results.Select(r => new RecoveryState(this, r.Key, r.Value));
        }

        /// <summary>
        /// <para>
        /// Scans the given key index to find any value that has a swap of the same index that decrypts the header block. Takes 60 of the index's 64 bits into consideration.
        /// </para>
        /// <para>
        /// This does not indicate that the value found is correct, but only indicates that the column swaps with itself. Popular swap columns appear to be 0 and 8, and these
        /// are often found to swap with themselves.
        /// </para>
        /// </summary>
        /// <param name="keyIndex"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<RecoveryState>> IndexSelfScan(int keyIndex, CancellationToken cancellationToken)
        {
            var results = await PartialKey.KeyIndexSelfScan(keyIndex, EncryptedHeaderBlock, PlaintextHeaderBlock, cancellationToken)
                .ConfigureAwait(false);

            return results.Select(r => new RecoveryState(this, r.Key, r.Value));
        }

        public void Write(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            EncryptedHeaderBlock.Write(writer);
            PlaintextHeaderBlock.Write(writer);

            writer.Write(B0Blocks.Count());

            foreach (var block in B0Blocks)
            {
                block.Write(writer);
            }

            writer.Write(SequentialBlocks.Count());

            foreach (var block in SequentialBlocks)
            {
                block.Write(writer);
            }
        }

        public static RecoveryState Read(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var newPartialKey = PartialKey.Read(reader);
            var newEncryptedHeaderBlock = Block.Read(reader);
            var newPlaintextHeaderBlock = Block.Read(reader);
            int size = reader.ReadInt32();

            var newB0Blocks = new List<Block>(size);

            for (int i = 0; i < size; i++)
            {
                newB0Blocks.Add(Block.Read(reader));
            }

            var newSequentialBlocks = new List<Block>(size);

            for (int i = 0; i < size; i++)
            {
                newSequentialBlocks.Add(Block.Read(reader));
            }

            return new RecoveryState(newPartialKey, newB0Blocks, newSequentialBlocks, newEncryptedHeaderBlock, newPlaintextHeaderBlock);
        }

        public int CompareTo(RecoveryState other)
        {
            int cmp = other.B0Blocks.Count() - B0Blocks.Count();

            if (cmp == 0)
            {
                cmp = other.SequentialBlocks.Count() - SequentialBlocks.Count();

                if (cmp == 0)
                {
                    cmp = PartialKey.CompareTo(other.PartialKey);
                }
            }

            return cmp;
        }

        public bool Equals(RecoveryState other)
        {
            if (other is null)
            {
                return false;
            }
            else
            {
                return PartialKey == other.PartialKey
                    && B0Blocks.Count() == other.B0Blocks.Count()
                    && SequentialBlocks.Count() == other.SequentialBlocks.Count();
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is RecoveryState r)
            {
                return Equals(r);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            var hashCode = 2040108761;
            hashCode = hashCode * -1521134295 + PartialKey.GetHashCode();
            hashCode = hashCode * -1521134295 + B0Blocks.Count().GetHashCode();
            hashCode = hashCode * -1521134295 + SequentialBlocks.Count().GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"RecoveryState({B0Blocks.Count()},{SequentialBlocks.Count()},{PartialKey.ToString()})";
        }

        public static bool operator ==(RecoveryState left, RecoveryState right)
        {
            return EqualityComparer<RecoveryState>.Default.Equals(left, right);
        }

        public static bool operator !=(RecoveryState left, RecoveryState right)
        {
            return !(left == right);
        }
    }
}
