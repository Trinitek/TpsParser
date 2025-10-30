using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TpsParser.Binary;
using TpsParser.Tps.KeyRecovery;

namespace TpsParser.Tests.Tps.KeyRecovery
{
    [TestFixture]
    public class BlockTest
    {
        private IEnumerable<Block> LoadBlocksFromFile(Stream stream, bool isEncrypted)
        {
            var ms = new MemoryStream();

            stream.CopyTo(ms);

            var rx = new TpsRandomAccess(ms.ToArray());

            var results = new List<Block>();

            while (!rx.IsAtEnd)
            {
                results.Add(new Block(rx, isEncrypted));
            }

            return results;
        }

        [Test]
        public void ShouldCreate()
        {
            var bA = new Block(offset: 0, values: new int[16], isEncrypted: true);
            var bB = new Block(offset: 10, bA);
            var bC = new Block(block: bA);
            var bD = bA.Apply(a: 0, b: 1, va: 1, vb: 2);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(bA.Offset, Is.Zero);
                Assert.That(bB.Offset, Is.EqualTo(10));
                Assert.That(bC.Offset, Is.Zero);
                Assert.That(bD.Offset, Is.Zero);

                Assert.That(bA.IsEncrypted, Is.True);
                Assert.That(bB.IsEncrypted, Is.True);
                Assert.That(bC.IsEncrypted, Is.True);
                Assert.That(bD.IsEncrypted, Is.True);

                Assert.That(bC, Is.EqualTo(bA), "A == C");
                Assert.That(bA, Is.EqualTo(bC), "C == A");
                Assert.That(bA, Is.Not.EqualTo(bB), "B != A");
                Assert.That(bA, Is.Not.EqualTo(bD), "D != A");

                Assert.That(bA.Values[0], Is.Zero);
                Assert.That(bD.Values[0], Is.EqualTo(1));
                Assert.That(bD.Values[1], Is.EqualTo(2));

                Assert.That(bA.CompareTo(bD), Is.EqualTo(-1));
                Assert.That(bD.CompareTo(bA), Is.EqualTo(1));
            }
        }

        [Test]
        public void ShouldLoadBlocks()
        {
            using (var fs = new FileStream("Resources/encrypted-a.tps", FileMode.Open))
            {
                var blocks = LoadBlocksFromFile(fs, isEncrypted: true);

                Assert.That(blocks.Count(), Is.EqualTo(24));

                var identicalBlocks = Block.FindIdenticalBlocks(blocks);

                Assert.That(identicalBlocks, Is.Not.Empty);
                Assert.That(identicalBlocks.Count, Is.EqualTo(4));
            }
        }

        [Test]
        public void ShouldGetHeaderIndexEndBlock()
        {
            using (var fs = new FileStream("Resources/encrypted-a.tps", FileMode.Open))
            {
                var blocks = LoadBlocksFromFile(fs, isEncrypted: true).ToList();

                var cryptBlock = Block.GetHeaderIndexEndBlock(blocks, isEncrypted: true);
                var plainBlock = Block.GetHeaderIndexEndBlock(blocks, isEncrypted: false);

                Assert.That(cryptBlock.Offset, Is.EqualTo(0x1C0));
                Assert.That(plainBlock.Offset, Is.EqualTo(0x1C0));

                Assert.That((uint)cryptBlock.Values[0], Is.EqualTo(0x925CDCB4));
                Assert.That((uint)plainBlock.Values[0], Is.EqualTo(0x00000004));
            }
        }

        [Test]
        public void ShouldGetSequenceBlock()
        {
            var sequenceBlock = Block.GenerateSequenceBlock(end: 0x5F5E5D5C);

            using (Assert.EnterMultipleScope())
            {
                Assert.That((uint)sequenceBlock.Values[0], Is.EqualTo(0x23222120));
                Assert.That((uint)sequenceBlock.Values[1], Is.EqualTo(0x27262524));
                Assert.That((uint)sequenceBlock.Values[14], Is.EqualTo(0x5B5A5958));
                Assert.That((uint)sequenceBlock.Values[15], Is.EqualTo(0x5F5E5D5C));
            }
        }

        [Test]
        public void ShouldGetSequenceBlockWithCycle()
        {
            var sequenceBlock = Block.GenerateSequenceBlock(end: 0x1F1E1D1C);

            using (Assert.EnterMultipleScope())
            {
                Assert.That((uint)sequenceBlock.Values[0], Is.EqualTo(0xE3E2E1E0));
                Assert.That((uint)sequenceBlock.Values[1], Is.EqualTo(0xE7E6E5E4));
                Assert.That((uint)sequenceBlock.Values[14], Is.EqualTo(0x1B1A1918));
                Assert.That((uint)sequenceBlock.Values[15], Is.EqualTo(0x1F1E1D1C));
            }
        }

        [Test]
        public void ShouldFindNoIdenticalBlocks()
        {
            var blocks = new Block[]
            {
                new Block(offset: 0, values: new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, isEncrypted: true),
                new Block(offset: 1, values: new int[] { 2, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, isEncrypted: true),
                new Block(offset: 2, values: new int[] { 3, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, isEncrypted: true),
                new Block(offset: 3, values: new int[] { 4, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, isEncrypted: true),
                new Block(offset: 4, values: new int[] { 5, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, isEncrypted: true)
            };

            var identicalBlocks = Block.FindIdenticalBlocks(blocks);

            Assert.That(identicalBlocks, Is.Empty);
        }

        [Test]
        public void ShouldFindIdenticalBlocks()
        {
            var blocks = new Block[]
            {
                new Block(offset: 0, values: new int[16], isEncrypted: true),
                new Block(offset: 1, values: new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, isEncrypted: true),
                new Block(offset: 2, values: new int[16], isEncrypted: true),
                new Block(offset: 3, values: new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 14 }, isEncrypted: true),
                new Block(offset: 4, values: new int[16], isEncrypted: true)
            };

            var identicalBlocks = Block.FindIdenticalBlocks(blocks);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(identicalBlocks, Is.Not.Empty);
                Assert.That(identicalBlocks.Count(), Is.EqualTo(1));
                Assert.That(identicalBlocks.First().Value.Count(), Is.EqualTo(2));
                Assert.That(identicalBlocks.First().Key, Is.EqualTo(blocks[0]));
            }
        }

        [Test]
        public void ShouldFindMultipleIdenticalBlocks()
        {
            var blocks = new Block[]
            {
                new Block(offset: 0, values: new int[16], isEncrypted: true),
                new Block(offset: 1, values: new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, isEncrypted: true),
                new Block(offset: 2, values: new int[16], isEncrypted: true),
                new Block(offset: 3, values: new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, isEncrypted: true),
                new Block(offset: 4, values: new int[16], isEncrypted: true)
            };

            var identicalBlocks = Block.FindIdenticalBlocks(blocks);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(identicalBlocks, Is.Not.Empty);
                Assert.That(identicalBlocks.Count(), Is.EqualTo(2));
                Assert.That(identicalBlocks.First().Value.Count(), Is.EqualTo(2));
                Assert.That(identicalBlocks.Last().Value.Count(), Is.EqualTo(1));
                Assert.That(identicalBlocks.First().Key, Is.EqualTo(blocks[0]));
                Assert.That(identicalBlocks.Last().Key, Is.EqualTo(blocks[1]));
            }
        }

        [Test]
        public void ShouldDetectSequencePart()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(Block.IsSequencePart(0x01021012), Is.False);
                Assert.That(Block.IsSequencePart(0x0201fffe), Is.False);
                Assert.That(Block.IsSequencePart(0x04030200), Is.False);

                Assert.That(Block.IsSequencePart(0x0f0e0d0c));
                Assert.That(Block.IsSequencePart(0x0100fffe));
                Assert.That(Block.IsSequencePart(0x00fffefd));
                Assert.That(Block.IsSequencePart(0x020100ff));
            }
        }

        [Test]
        public void ShouldDetectB0B0Part()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(Block.IsB0Part(0x04030200), Is.False);

                Assert.That(Block.IsB0Part(0xB0B0B0B0));
            }
        }
    }
}
