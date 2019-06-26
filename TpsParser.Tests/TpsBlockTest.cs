using NUnit.Framework;
using TpsParser.Binary;
using TpsParser.Tps;

namespace TpsParser.Tests
{
    [TestFixture]
    public class TpsBlockTest
    {
        [Test]
        public void ShouldReadTwoBlocks()
        {
            var rx = new RandomAccess(new byte[4 * 256]);

            rx.WriteLongLE(0);
            rx.WriteLongLE(0x200);

            rx.JumpAbsolute(0x200);
            rx.WriteLongLE(0x200);
            rx.WriteLongLE(0x100);

            rx.JumpAbsolute(0);

            var block = new TpsBlock(rx, 0, 0x300, false);

            Assert.AreEqual(2, block.Pages.Count);
            Assert.AreEqual(0x200, block.Pages[0].Size);
            Assert.AreEqual(0x100, block.Pages[1].Size);
        }

        [Test]
        public void ShouldReadTwoBlocksWithGap()
        {
            var rx = new RandomAccess(new byte[4 * 256]);

            rx.WriteLongLE(0);
            rx.WriteLongLE(0x100);

            rx.JumpAbsolute(0x200);
            rx.WriteLongLE(0x200);
            rx.WriteLongLE(0x100);

            rx.JumpAbsolute(0);

            var block = new TpsBlock(rx, 0, 0x300, false);

            Assert.AreEqual(2, block.Pages.Count);
            Assert.AreEqual(0x100, block.Pages[0].Size);
            Assert.AreEqual(0x100, block.Pages[1].Size);
        }

        [Test]
        public void ShouldSkipPartiallyOverwrittenBlock()
        {
            var rx = new RandomAccess(new byte[4 * 256]);

            rx.WriteLongLE(0);
            rx.WriteLongLE(0x300);

            rx.JumpAbsolute(0x100); // Inside the previous block!
            rx.WriteLongLE(0x100);
            rx.WriteLongLE(0x200);

            rx.JumpAbsolute(0);

            var block = new TpsBlock(rx, 0, 0x300, false);

            Assert.AreEqual(1, block.Pages.Count);
            Assert.AreEqual(0x100, block.Pages[0].Address);
            Assert.AreEqual(0x200, block.Pages[0].Size);
        }
    }
}
