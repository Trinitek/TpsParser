using NUnit.Framework;
using System.Text;
using TpsParser.Binary;
using TpsParser.Tps;

namespace TpsParser.Tests.Tps;

[TestFixture]
internal sealed class TpsBlockTest
{
    [Test]
    public void ShouldReadTwoBlocks()
    {
        var rx = new TpsRandomAccess(new byte[4 * 256], Encoding.ASCII);

        rx.WriteLongLE(0);
        rx.WriteLongLE(0x200);

        rx.JumpAbsolute(0x200);
        rx.WriteLongLE(0x200);
        rx.WriteLongLE(0x100);

        rx.JumpAbsolute(0);

        var block = new TpsBlock(rx, new TpsPageDescriptor(0, 0x300), false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(block.Pages, Has.Count.EqualTo(2));
            Assert.That(block.Pages[0].Size, Is.EqualTo(0x200));
            Assert.That(block.Pages[1].Size, Is.EqualTo(0x100));
        }
    }

    [Test]
    public void ShouldReadTwoBlocksWithGap()
    {
        var rx = new TpsRandomAccess(new byte[4 * 256], Encoding.ASCII);

        rx.WriteLongLE(0);
        rx.WriteLongLE(0x100);

        rx.JumpAbsolute(0x200);
        rx.WriteLongLE(0x200);
        rx.WriteLongLE(0x100);

        rx.JumpAbsolute(0);

        var block = new TpsBlock(rx, new TpsPageDescriptor(0, 0x300), false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(block.Pages, Has.Count.EqualTo(2));
            Assert.That(block.Pages[0].Size, Is.EqualTo(0x100));
            Assert.That(block.Pages[1].Size, Is.EqualTo(0x100));
        }
    }

    [Test]
    public void ShouldSkipPartiallyOverwrittenBlock()
    {
        var rx = new TpsRandomAccess(new byte[4 * 256], Encoding.ASCII);

        rx.WriteLongLE(0);
        rx.WriteLongLE(0x300);

        rx.JumpAbsolute(0x100); // Inside the previous block!
        rx.WriteLongLE(0x100);
        rx.WriteLongLE(0x200);

        rx.JumpAbsolute(0);

        var block = new TpsBlock(rx, new TpsPageDescriptor(0, 0x300), false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(block.Pages, Has.Count.EqualTo(1));
            Assert.That(block.Pages[0].Address, Is.EqualTo(0x100));
            Assert.That(block.Pages[0].Size, Is.EqualTo(0x200));
        }
    }
}
