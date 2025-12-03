using NUnit.Framework;
using System.Text;

namespace TpsParser.RandomAccess.Tests;

internal sealed class WriteData
{
    [Test]
    public void ShouldWriteLE()
    {
        var ra = new TpsRandomAccess([1, 2, 3, 4], Encoding.ASCII);
        int value = ra.ReadLongLE();
        ra.JumpAbsolute(0);
        ra.WriteLongLE(value);
        ra.JumpAbsolute(0);
        int value2 = ra.ReadLongLE();

        Assert.That(value2, Is.EqualTo(value));
    }
}
