using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TpsParser.Binary;

namespace TpsParser.Tps.KeyRecovery
{
    /// <summary>
    /// Represents a partial encryption/decryption key. This class provides methods for solving pieces of the key
    /// by comparing decrypted blocks with encrypted blocks.
    /// </summary>
    public sealed class PartialKey : IComparable<PartialKey>, IEquatable<PartialKey>
    {
        private IReadOnlyList<bool> Valid { get; }
        private IReadOnlyList<int> KeyPiece { get; }

        /// <summary>
        /// Returns true if all of the key pieces are present and have been validated.
        /// </summary>
        public bool IsComplete
        {
            get
            {
                foreach (var v in Valid)
                {
                    if (!v)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Instantiates a new partial key.
        /// </summary>
        public PartialKey()
            : this(
                  valid: new bool[16],
                  key: new int[16])
        { }

        private PartialKey(IReadOnlyList<bool> valid, IReadOnlyList<int> key)
        {
            Valid = valid ?? throw new ArgumentNullException(nameof(valid));
            KeyPiece = key ?? throw new ArgumentNullException(nameof(key));
        }

        private PartialKey(PartialKey partialKey, int index, int value)
        {
            if (partialKey == null)
            {
                throw new ArgumentNullException(nameof(partialKey));
            }

            var newValid = partialKey.Valid.ToArray();
            var newKeys = partialKey.KeyPiece.ToArray();

            newValid[index] = true;
            newKeys[index] = value;

            Valid = newValid;
            KeyPiece = newKeys;
        }

        /// <summary>
        /// <para>
        /// Attempts to find matching key values for the given index by matching a block
        /// of crypttext with plaintext.
        /// </para>
        /// <para>
        /// This only works if there are no other key indexes
        /// with a swap for this index.  For index 0x0F it always works because none of
        /// the other indexes will select index 0x0F.
        /// </para>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="encryptedBlock"></param>
        /// <param name="plaintextBlock"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<IReadOnlyDictionary<PartialKey, Block>> KeyIndexScan(
            int index,
            Block encryptedBlock,
            Block plaintextBlock,
            CancellationToken cancellationToken) =>
            InternalKeyIndexScan(
                index,
                encryptedBlock,
                plaintextBlock,
                checkKeyIndex: false,
                cancellationToken: cancellationToken);

        /// <summary>
        /// Attempts to find key values that have their swap column set at their own index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="encryptedBlock"></param>
        /// <param name="plaintextBlock"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<IReadOnlyDictionary<PartialKey, Block>> KeyIndexSelfScan(
            int index,
            Block encryptedBlock,
            Block plaintextBlock,
            CancellationToken cancellationToken) =>
            InternalKeyIndexScan(
                index,
                encryptedBlock,
                plaintextBlock,
                checkKeyIndex: true,
                cancellationToken: cancellationToken);

        private async Task<IReadOnlyDictionary<PartialKey, Block>> InternalKeyIndexScan(
            int index,
            Block encryptedBlock,
            Block plaintextBlock,
            bool checkKeyIndex,
            CancellationToken cancellationToken)
        {
            if (encryptedBlock == null)
            {
                throw new ArgumentNullException(nameof(encryptedBlock));
            }

            if (plaintextBlock == null)
            {
                throw new ArgumentNullException(nameof(plaintextBlock));
            }

            var results = new ConcurrentDictionary<PartialKey, Block>();

            // Evenly split the work among the number of threads in the threadpool.

            ThreadPool.GetMinThreads(out int threadCount, out int _);

            var bruteforcerTasks = new Task[threadCount];

            for (int thread = 0; thread < threadCount; thread++)
            {
                uint valuesPerTask = uint.MaxValue / (uint)threadCount;
                uint startValue = valuesPerTask * (uint)thread;
                uint stopValue = startValue + valuesPerTask - 1;

                if (thread == threadCount - 1)
                {
                    stopValue += uint.MaxValue % (uint)threadCount + 1;
                }

                bruteforcerTasks[thread] = BruteforceScan(
                    results,
                    encryptedBlock,
                    plaintextBlock,
                    index,
                    checkKeyIndex,
                    startValue,
                    stopValue,
                    cancellationToken);
            }

            await Task.WhenAll(bruteforcerTasks).ConfigureAwait(false);

            return results;
        }

        private async Task BruteforceScan(
            IDictionary<PartialKey, Block> results,
            Block encryptedBlock,
            Block plaintextBlock,
            int index,
            bool checkKeyIndex,
            uint startValue,
            uint stopValue,
            CancellationToken cancellationToken)
        {
            await Task.Yield();

            int positionA = index;
            int plain = plaintextBlock.Values[positionA];

            for (long v = startValue; v <= stopValue; v++)
            {
                int keyA = (int)v;
                int positionB = keyA & 0x0F;

                if (checkKeyIndex)
                {
                    if (positionB != index)
                    {
                        continue;
                    }
                }

                int data1 = encryptedBlock.Values[positionA] - keyA;
                int data2 = encryptedBlock.Values[positionB] - keyA;

                int opAnd1 = data1 & keyA;
                int opNotA = ~keyA;
                int opAnd2 = data2 & opNotA;
                int opOr1 = opAnd1 | opAnd2;

                if (opOr1 == plain)
                {
                    int opAnd3 = data2 & keyA;
                    int opAnd4 = data1 & opNotA;
                    int opOr2 = opAnd3 | opAnd4;

                    var decryptedBlock = encryptedBlock.Apply(positionA, positionB, opOr1, opOr2);
                    var partialKey = Apply(index, keyA);

                    results.Add(partialKey, decryptedBlock);
                }

                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        /// <summary>
        /// Appends a solved key piece to the current list of pieces and returns a new partial key.
        /// </summary>
        /// <param name="index">The index at which the piece belongs in the final key.</param>
        /// <param name="keyPiece">The solved key piece.</param>
        /// <returns></returns>
        public PartialKey Apply(int index, int keyPiece) => new PartialKey(this, index, keyPiece);

        /// <summary>
        /// Returns a completed key.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the partial key is not complete. See <see cref="IsComplete"/>.</exception>
        /// <returns></returns>
        public Key ToKey()
        {
            if (IsComplete)
            {
                var rx = new RandomAccess(new byte[64]);

                foreach (var k in KeyPiece)
                {
                    rx.WriteLongLE(k);
                }

                return new Key(rx);
            }
            else
            {
                throw new InvalidOperationException("Incomplete PartialKey.");
            }
        }

        public Block PartialDecrypt(int index, Block block)
        {
            if (!Valid[index])
            {
                throw new ArgumentException("The given index was not valid.", nameof(index));
            }

            int keyA = KeyPiece[index];
            int positionA = index;
            int positionB = keyA & 0x0F;

            int dataA = block.Values[positionA] - keyA;
            int dataB = block.Values[positionB] - keyA;

            int opAnd1 = dataA & keyA;
            int opNot1 = ~keyA;
            int opAnd2 = dataB & opNot1;
            int opOr1 = opAnd1 | opAnd2;

            int opAnd3 = dataB & keyA;
            int opAnd4 = dataA & opNot1;
            int opOr2 = opAnd3 | opAnd4;

            return block.Apply(positionA, positionB, opOr1, opOr2);
        }

        public IReadOnlyList<int> GetInvalidIndexes()
        {
            int count = Valid.Sum(v => v ? 0 : 1);

            var indexes = new int[count];

            for (int i = 0; i < Valid.Count; i++)
            {
                if (!Valid[i])
                {
                    indexes[--count] = i;
                }
            }

            return indexes;
        }

        public void Write(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.Write(Valid.Count);

            for (int i = 0; i < Valid.Count; i++)
            {
                writer.Write(Valid[i]);
                writer.Write(KeyPiece[i]);
            }
        }

        public static PartialKey Read(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var length = reader.ReadInt32();

            var newValid = new bool[length];
            var newKey = new int[length];

            for (int i = 0; i < length; i++)
            {
                newValid[i] = reader.ReadBoolean();
                newKey[i] = reader.ReadInt32();
            }

            return new PartialKey(newValid, newKey);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override string ToString()
        {
            var sb = new StringBuilder();

            for (int i = 0; i < Valid.Count; i++)
            {
                if (Valid[i])
                {
                    sb.Append($"{KeyPiece[i]:x8} ");
                }
                else
                {
                    sb.Append("???????? ");
                }
            }

            return sb.ToString();
        }

        public int CompareTo(PartialKey other)
        {
            for (int i = 0; i < KeyPiece.Count; i++)
            {
                int d = KeyPiece[i] - other.KeyPiece[i];

                if (d != 0)
                {
                    return d;
                }

                d = (Valid[i] ? 1 : 0) - (other.Valid[i] ? 1 : 0);

                if (d != 0)
                {
                    return d;
                }
            }

            return 0;
        }

        public bool Equals(PartialKey other)
        {
            if (other is null)
            {
                return false;
            }
            else
            {
                return Valid.SequenceEqual(other.Valid)
                    && KeyPiece.SequenceEqual(other.KeyPiece);
            }
        }

        public override bool Equals(object obj) => obj is PartialKey p ? Equals(p) : false;

        public override int GetHashCode()
        {
            var hashCode = -1385299771;

            foreach (var v in Valid)
            {
                hashCode = hashCode * -1521134295 + v.GetHashCode();
            }

            foreach (var k in KeyPiece)
            {
                hashCode = hashCode * -1521134295 + k.GetHashCode();
            }

            return hashCode;
        }

        public static bool operator ==(PartialKey left, PartialKey right)
        {
            return EqualityComparer<PartialKey>.Default.Equals(left, right);
        }

        public static bool operator !=(PartialKey left, PartialKey right)
        {
            return !(left == right);
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
