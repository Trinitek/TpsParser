using System.Collections.Generic;

namespace TpsParser.Tps.KeyRecovery
{
    internal sealed class BlockValueEqualityComparer : IEqualityComparer<Block>
    {
        public static readonly BlockValueEqualityComparer Instance = new BlockValueEqualityComparer();

        public bool Equals(Block x, Block y) => x.ValueEquals(y);

        public int GetHashCode(Block obj) => 0;
    }
}
