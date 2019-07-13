using System.Collections.Generic;
using System.Linq;

namespace TpsParser.Tps.KeyRecovery
{
    public static class RecoveryStateExtensions
    {
        /// <summary>
        /// <para>
        /// Reduces the number of candidate solutions by finding any blocks that decrypt to a sequence (like 0x40414243).
        /// </para>
        /// <para>
        /// Blocks that are found to hold a sequence are added to the recovery state for further evaluation.
        /// </para>
        /// </summary>
        /// <param name="candidates"></param>
        /// <param name="index"></param>
        /// <param name="blocks"></param>
        /// <returns></returns>
        public static IEnumerable<RecoveryState> ReduceFirstSequential(this IEnumerable<RecoveryState> candidates, int index, IEnumerable<Block> blocks) =>
            candidates
                .Select(s => InnerReduceSequential(s, index, blocks))
                .Where(s => s != null);

        /// <summary>
        /// Reduces the number of candidate solutions by finding any blocks that decrypt to a sequence (like 0x40414243).
        /// </summary>
        /// <param name="candidates"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static IEnumerable<RecoveryState> ReduceNextSequential(this IEnumerable<RecoveryState> candidates, int index) =>
            candidates
                .Select(s => InnerReduceSequential(s, index, s.SequentialBlocks))
                .Where(s => s != null);

        private static RecoveryState InnerReduceSequential(RecoveryState state, int index, IEnumerable<Block> blocks)
        {
            var sequences = blocks
                .Select(block => state.PartialKey.PartialDecrypt(index, block))
                .Where(decryptedBlock => Block.IsSequencePart(decryptedBlock.Values[index]));

            return sequences.Any() ? RecoveryState.CreateFromSequentialBlocks(state, sequences) : null;
        }

        /// <summary>
        /// <para>
        /// Reduces the number of solutions by attempting to decrypt blocks with duplicates of 0xB0B0B0B0 blocks.
        /// </para>
        /// <para>
        /// If at least one of those blocks is found, the solution is kept. The blocks that decrypt to 0xB0B0 are saved in the keys
        /// recovery state for further decryption.
        /// </para>
        /// </summary>
        /// <param name="candidates"></param>
        /// <param name="index"></param>
        /// <param name="blocks"></param>
        /// <returns></returns>
        public static IEnumerable<RecoveryState> ReduceFirstB0(this IEnumerable<RecoveryState> candidates, int index, IEnumerable<Block> blocks) =>
            candidates
                .Select(s => InnerReduceB0(s, index, blocks))
                .Where(s => s != null);

        /// <summary>
        /// <para>
        /// Reduces the number of solutions by re-evaluating the found 0xB0B0 blocks at this index.
        /// </para>
        /// <para>
        /// If at least one block still decrypts to 0xB0B0, the candidate is kept.
        /// </para>
        /// </summary>
        /// <param name="candidates"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static IEnumerable<RecoveryState> ReduceNextB0(this IEnumerable<RecoveryState> candidates, int index) =>
            candidates
                .Select(s => InnerReduceB0(s, index, s.B0Blocks))
                .Where(s => s != null);

        private static RecoveryState InnerReduceB0(RecoveryState state, int index, IEnumerable<Block> blocks)
        {
            var b0s = blocks
                .Select(block => state.PartialKey.PartialDecrypt(index, block))
                .Where(decryptedBlock => Block.IsB0Part((uint)decryptedBlock.Values[index]));

            return b0s.Any() ? RecoveryState.CreateFromB0Blocks(state, b0s) : null;
        }

        public static IEnumerable<RecoveryState> ReduceFirst(this IEnumerable<RecoveryState> candidates, int index, IEnumerable<Block> blocks) =>
            candidates
                .ReduceFirstB0(index, blocks)
                .ReduceFirstSequential(index, blocks);

        public static IEnumerable<RecoveryState> ReduceNext(this IEnumerable<RecoveryState> candidates, int index) =>
            candidates
                .ReduceNextB0(index)
                .ReduceNextSequential(index);
    }
}
