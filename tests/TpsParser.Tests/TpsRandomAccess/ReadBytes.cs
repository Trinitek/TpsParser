using NUnit.Framework;
using System.Text;
using TpsParser.Binary;

namespace TpsParser.RandomAccess.Tests;

internal sealed class ReadBytes
{
    [Test]
    public void ShouldReadAsMemory_FullLength()
    {
        var rx = new TpsRandomAccess([1, 2, 3, 4, 5, 6, 7, 8], Encoding.ASCII);
        var rom = rx.ReadBytesAsMemory(length: 8);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(rom.ToArray(), Is.EqualTo([1, 2, 3, 4, 5, 6, 7, 8]));
            Assert.That(rx.Position, Is.EqualTo(8));
            Assert.That(rx.IsAtEnd, Is.True);
        }
    }

    [Test]
    public void ShouldReadAsMemory_Partial_Read4bytes()
    {
        var rx = new TpsRandomAccess([1, 2, 3, 4, 5, 6, 7, 8], Encoding.ASCII);
        var rom = rx.ReadBytesAsMemory(length: 4);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(rom.ToArray(), Is.EqualTo([1, 2, 3, 4]));
            Assert.That(rx.Position, Is.EqualTo(4));
            Assert.That(rx.IsAtEnd, Is.False);
        }
    }

    [Test]
    public void ShouldReadAsMemory_Partial_Jump2_Read4bytes()
    {
        var rx = new TpsRandomAccess([1, 2, 3, 4, 5, 6, 7, 8], Encoding.ASCII);

        rx.JumpAbsolute(2);

        var rom = rx.ReadBytesAsMemory(length: 4);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(rom.ToArray(), Is.EqualTo([3, 4, 5, 6]));
            Assert.That(rx.Position, Is.EqualTo(6));
            Assert.That(rx.IsAtEnd, Is.False);
        }
    }

    [Test]
    public void ShouldReadAsMemory_Partial_Jump2_Read6Bytes()
    {
        var rx = new TpsRandomAccess([1, 2, 3, 4, 5, 6, 7, 8], Encoding.ASCII);

        rx.JumpAbsolute(2);

        var rom = rx.ReadBytesAsMemory(length: 6);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(rom.ToArray(), Is.EqualTo([3, 4, 5, 6, 7, 8]));
            Assert.That(rx.Position, Is.EqualTo(8));
            Assert.That(rx.IsAtEnd, Is.True);
        }
    }

    [Test]
    public void ShouldReadAsMemory_Partial_BaseOffset2_Read4bytes()
    {
        var rx = new TpsRandomAccess([1, 2, 3, 4, 5, 6, 7, 8], baseOffset: 2, length: 6, Encoding.ASCII);
        var rom = rx.ReadBytesAsMemory(length: 4);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(rom.ToArray(), Is.EqualTo([3, 4, 5, 6]));
            Assert.That(rx.Position, Is.EqualTo(4));
            Assert.That(rx.IsAtEnd, Is.False);
        }
    }

    [Test]
    public void ShouldReadAsMemory_Partial_BaseOffset4_Read4bytes()
    {
        var rx = new TpsRandomAccess([1, 2, 3, 4, 5, 6, 7, 8], baseOffset: 4, length: 4, Encoding.ASCII);
        var rom = rx.ReadBytesAsMemory(length: 4);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(rom.ToArray(), Is.EqualTo([5, 6, 7, 8]));
            Assert.That(rx.Position, Is.EqualTo(4));
            Assert.That(rx.IsAtEnd, Is.True);
        }
    }
}
