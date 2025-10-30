using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TpsParser.Binary;
using TpsParser.Tps;
using TpsParser.Tps.KeyRecovery;

namespace TpsParser.Tests.Tps.KeyRecovery
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

            k.Encrypt64(new TpsRandomAccess(crypt));
            k.Encrypt64(new TpsRandomAccess(cb0));
            k.Encrypt64(new TpsRandomAccess(cseq));

            var plaintext = new Block(new TpsRandomAccess(plain), isEncrypted: false);
            var encrypted = new Block(new TpsRandomAccess(crypt), isEncrypted: true);

            var cryptSeq = new Block(new TpsRandomAccess(cseq), isEncrypted: false);
            var cryptB0 = new Block(new TpsRandomAccess(cb0), isEncrypted: false);

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

        [Test]
        public async Task ShouldScanAndReduce()
        {
            (var plaintext, var encrypted, var blocks) = BuildBlocks();

            var state = new RecoveryState(encrypted, plaintext);

            var scanResults = await state.IndexScan(keyIndex: 15, cancellationToken: default);
            Assert.AreEqual(192, scanResults.Count());

            var firstReducedScanResults = scanResults.ReduceFirst(index: 15, blocks: blocks);
            Assert.AreEqual(2, firstReducedScanResults.Count());

            VerifyReadWrite(firstReducedScanResults.First());

            var secondReducedScanResults = await firstReducedScanResults.First().IndexScan(keyIndex: 14, cancellationToken: default);
            Assert.AreEqual(1450, secondReducedScanResults.Count());

            var thirdReducedScanResults = secondReducedScanResults.ReduceNext(index: 14);
            Assert.AreEqual(2, thirdReducedScanResults.Count());
        }

        private void VerifyReadWrite(RecoveryState recoveryState)
        {
            var memoryStream = new MemoryStream();
            var writer = new BinaryWriter(memoryStream);

            recoveryState.Write(writer);

            memoryStream.Position = 0;

            var reader = new BinaryReader(memoryStream);

            var stateCopy = RecoveryState.Read(reader);

            Assert.AreEqual(stateCopy, recoveryState);
        }
    }
}
