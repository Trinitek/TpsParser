using NUnit.Framework;
using System;
using System.Text;
using TpsParser.Binary;

namespace TpsParser.RandomAccess.Tests;

internal sealed class BufferBoundaries
{
    [Test]
    public void ShouldFailBeyondBuffer()
    {
        var ra = new TpsRandomAccess([1, 2, 3, 4], Encoding.ASCII);
        ra.ReadByte();
        var read = ra.Read(3);

        Assert.Throws<IndexOutOfRangeException>(() => read.ReadLongLE());
    }

    [Test]
    public void ShouldFailBeforeBuffer()
    {
        var ra = new TpsRandomAccess([1, 2, 3, 4], Encoding.ASCII);
        ra.ReadByte();
        var read = ra.Read(3);
        read.JumpAbsolute(-1);

        Assert.Throws<IndexOutOfRangeException>(() => read.ReadByte());
    }
}
