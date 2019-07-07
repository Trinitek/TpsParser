using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TpsParser.Binary;
using TpsParser.Tps;
using TpsParser.Tps.KeyRecovery;

namespace TpsParser.Tests.KeyRecovery
{
    [TestFixture]
    public class RecoveryStateTest
    {
        private (Block plaintext, Block encrypted, IEnumerable<Block> blocks) BuildBlocks()
        {
            var k = new Key("nasigoreng");

            var plain = new byte[64];
            var crypt = new byte[64];
            var cseq = Enumerable.Range(0, 64).Select(i => (byte)i).ToArray();
            var cb0 = Enumerable.Repeat<byte>(0xB0, 64).ToArray();

            k.Encrypt64(new RandomAccess(crypt));
            k.Encrypt64(new RandomAccess(cb0));
            k.Encrypt64(new RandomAccess(cseq));

            var plaintext = new Block(new RandomAccess(plain), isEncrypted: false);
            var encrypted = new Block(new RandomAccess(crypt), isEncrypted: true);

            var cryptSeq = new Block(new RandomAccess(cseq), isEncrypted: false);
            var cryptB0 = new Block(new RandomAccess(cb0), isEncrypted: false);

            var blocks = new Block[]
            {
                encrypted,
                new Block(0x400, cryptB0),
                new Block(0x500, cryptSeq),
                new Block(0x600, cryptB0),
                new Block(0x700, cryptSeq),
                new Block(0x800, cryptB0),
                new Block(0xA00, cryptSeq),
            };

            return (plaintext, encrypted, blocks);
        }

        [Test]
        public async Task ShouldDoSelfScan()
        {
            (var plaintext, var encrypted, var _) = BuildBlocks();

            var state = new RecoveryState(encrypted, plaintext);

            var selfScan1 = await state.IndexSelfScan(keyIndex: 8, cancellationToken: default);
            var selfScan2 = await state.IndexSelfScan(keyIndex: 15, cancellationToken: default);

            Assert.AreEqual(1, selfScan1.Count());
            Assert.AreEqual(0, selfScan2.Count());
        }
    }
}
