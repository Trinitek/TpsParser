using NUnit.Framework;
using System.Text;
using TpsParser.Tps;

namespace TpsParser.Tests.Tps;

[TestFixture]
internal sealed class TestTpsBlock
{
    [Test]
    public void ShouldReadTwoBlocks()
    {
        //byte[] data = [
        //    /* Block[0] */
        //
        //    /* Block[0]Page[0] */
        //    0x00, 0x00, 0x00, 0x00, /* AbsoluteAddress */
        //    0x00, 0x02,             /* Size */
        //    0x00, 0x02,             /* SizeUncompressed */
        //    0x00, 0x01,             /* RecordCount */
        //    0x00,                   /* Flags */
        //
        //    ..Enumerable.Repeat<byte>(0xff, 0x200 - 11),    /* Page data (junk) */
        //
        //    /* Block[1] */
        //
        //    /* Block[1]Page[0] */
        //    0x00, 0x00, 0x02, 0x00, /* AbsoluteAddress */
        //    0x00, 0x02,             /* Size */
        //    0x00, 0x02,             /* SizeUncompressed */
        //    0x00, 0x01,             /* RecordCount */
        //    0x00,                   /* Flags */
        //
        //    ..Enumerable.Repeat<byte>(0xee, 0x200 - 11),    /* Page data (junk) */
        //    ];
        //
        //var rx = new TpsRandomAccess(data, Encoding.ASCII);

        var rx = new TpsRandomAccess(new byte[4 * 256], Encoding.ASCII);
        
        rx.WriteLongLE(0);
        rx.WriteLongLE(0x200);
        
        rx.JumpAbsolute(0x200);
        rx.WriteLongLE(0x200);
        rx.WriteLongLE(0x100);
        
        rx.JumpAbsolute(0);


        var block = TpsBlock.Parse(new TpsBlockDescriptor(0, 0x300), rx);

        var pages = block.GetPages(ignorePageErrors: false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(pages, Has.Count.EqualTo(2));
            Assert.That(pages[0].Size, Is.EqualTo(0x200));
            Assert.That(pages[1].Size, Is.EqualTo(0x100));
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

        var block = TpsBlock.Parse(new TpsBlockDescriptor(0, 0x300), rx);

        var pages = block.GetPages(ignorePageErrors: false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(pages, Has.Count.EqualTo(2));
            Assert.That(pages[0].Size, Is.EqualTo(0x100));
            Assert.That(pages[1].Size, Is.EqualTo(0x100));
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

        var block = TpsBlock.Parse(new TpsBlockDescriptor(0, 0x300), rx);

        var pages = block.GetPages(ignorePageErrors: false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(pages, Has.Count.EqualTo(1));
            Assert.That(pages[0].AbsoluteAddress, Is.EqualTo(0x100));
            Assert.That(pages[0].Size, Is.EqualTo(0x200));
        }
    }
}
