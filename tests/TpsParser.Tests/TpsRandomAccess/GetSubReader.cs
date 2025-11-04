using NUnit.Framework;
using System.Text;
using TpsParser.Binary;

namespace TpsParser.RandomAccess.Tests;

internal sealed class GetSubReader
{
    [Test]
    public void ReadShouldReturnNewRxAndReuseBuffer()
    {
        var ra = new TpsRandomAccess([1, 2, 3, 4], Encoding.ASCII);
        ra.ReadByte();
        var read = ra.Read(3);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(read.Encoding, Is.EqualTo(Encoding.ASCII));
            Assert.That(read.Length, Is.EqualTo(3));
            Assert.That(read.Position, Is.Zero);
            Assert.That(read.ReadByte(), Is.EqualTo(2));
            Assert.That(read.ReadByte(), Is.EqualTo(3));
            Assert.That(read.ReadByte(), Is.EqualTo(4));
        }

        read.JumpAbsolute(0);
        Assert.That(read.ReadByte(), Is.EqualTo(2));

        read.JumpRelative(1);
        Assert.That(read.ReadByte(), Is.EqualTo(4));

        read.JumpAbsolute(1);

        var read2 = read.Read(2);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(read2.Encoding, Is.EqualTo(Encoding.ASCII));
            Assert.That(read2.Length, Is.EqualTo(2));
            Assert.That(read2.Position, Is.Zero);
            Assert.That(read2.ReadByte(), Is.EqualTo(3));
            Assert.That(read2.ReadByte(), Is.EqualTo(4));
        }
    }
}
