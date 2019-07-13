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

            var rx = new RandomAccess(ms.ToArray());

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

            Assert.AreEqual(0, bA.Offset);
            Assert.AreEqual(10, bB.Offset);
            Assert.AreEqual(0, bC.Offset);
            Assert.AreEqual(0, bD.Offset);

            Assert.True(bA.IsEncrypted);
            Assert.True(bB.IsEncrypted);
            Assert.True(bC.IsEncrypted);
            Assert.True(bD.IsEncrypted);

            Assert.AreEqual(bA, bC, "A == C");
            Assert.AreEqual(bC, bA, "C == A");
            Assert.AreNotEqual(bB, bA, "B != A");
            Assert.AreNotEqual(bD, bA, "D != A");

            Assert.AreEqual(0, bA.Values[0]);
            Assert.AreEqual(1, bD.Values[0]);
            Assert.AreEqual(2, bD.Values[1]);

            Assert.AreEqual(-1, bA.CompareTo(bD));
            Assert.AreEqual(1, bD.CompareTo(bA));
        }

        [Test]
        public void ShouldLoadBlocks()
        {
            using (var fs = new FileStream("Resources/encrypted-a.tps", FileMode.Open))
            {
                var blocks = LoadBlocksFromFile(fs, isEncrypted: true);

                Assert.AreEqual(24, blocks.Count());

                var identicalBlocks = Block.FindIdenticalBlocks(blocks);

                CollectionAssert.IsNotEmpty(identicalBlocks);
                Assert.AreEqual(4, identicalBlocks.Count);
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

                Assert.AreEqual(0x1C0, cryptBlock.Offset);
                Assert.AreEqual(0x1C0, plainBlock.Offset);

                Assert.AreEqual(0x925CDCB4, (uint)cryptBlock.Values[0]);
                Assert.AreEqual(0x00000004, (uint)plainBlock.Values[0]);
            }
        }

        [Test]
        public void ShouldGetSequenceBlock()
        {
            var sequenceBlock = Block.GenerateSequenceBlock(end: 0x5F5E5D5C);

            Assert.AreEqual(0x23222120, (uint)sequenceBlock.Values[0]);
            Assert.AreEqual(0x27262524, (uint)sequenceBlock.Values[1]);
            Assert.AreEqual(0x5B5A5958, (uint)sequenceBlock.Values[14]);
            Assert.AreEqual(0x5F5E5D5C, (uint)sequenceBlock.Values[15]);
        }

        [Test]
        public void ShouldGetSequenceBlockWithCycle()
        {
            var sequenceBlock = Block.GenerateSequenceBlock(end: 0x1F1E1D1C);

            Assert.AreEqual(0xE3E2E1E0, (uint)sequenceBlock.Values[0]);
            Assert.AreEqual(0xE7E6E5E4, (uint)sequenceBlock.Values[1]);
            Assert.AreEqual(0x1B1A1918, (uint)sequenceBlock.Values[14]);
            Assert.AreEqual(0x1F1E1D1C, (uint)sequenceBlock.Values[15]);
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

            CollectionAssert.IsEmpty(identicalBlocks);
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

            CollectionAssert.IsNotEmpty(identicalBlocks);
            Assert.AreEqual(1, identicalBlocks.Count());
            Assert.AreEqual(2, identicalBlocks.First().Value.Count());
            Assert.AreEqual(blocks[0], identicalBlocks.First().Key);
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

            CollectionAssert.IsNotEmpty(identicalBlocks);
            Assert.AreEqual(2, identicalBlocks.Count());
            Assert.AreEqual(2, identicalBlocks.First().Value.Count());
            Assert.AreEqual(1, identicalBlocks.Last().Value.Count());
            Assert.AreEqual(blocks[0], identicalBlocks.First().Key);
            Assert.AreEqual(blocks[1], identicalBlocks.Last().Key);
        }

        [Test]
        public void ShouldDetectSequencePart()
        {
            Assert.IsFalse(Block.IsSequencePart(0x01021012));
            Assert.IsFalse(Block.IsSequencePart(0x0201fffe));
            Assert.IsFalse(Block.IsSequencePart(0x04030200));

            Assert.IsTrue(Block.IsSequencePart(0x0f0e0d0c));
            Assert.IsTrue(Block.IsSequencePart(0x0100fffe));
            Assert.IsTrue(Block.IsSequencePart(0x00fffefd));
            Assert.IsTrue(Block.IsSequencePart(0x020100ff));
        }

        [Test]
        public void ShouldDetectB0B0Part()
        {
            Assert.IsFalse(Block.IsB0Part(0x04030200));

            Assert.IsTrue(Block.IsB0Part(0xB0B0B0B0));
        }
    }
}
