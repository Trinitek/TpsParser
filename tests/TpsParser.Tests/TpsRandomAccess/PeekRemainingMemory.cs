using NUnit.Framework;
using System.Text;

namespace TpsParser.RandomAccess.Tests;

internal sealed class PeekRemainingMemory
{
    [Test]
    public void EmptyData_ShouldReturnEmptyMemory()
    {
        var rx = new TpsRandomAccess([], Encoding.ASCII);

        var mem = rx.PeekRemainingMemory();

        Assert.That(mem.IsEmpty, Is.True);
    }

    [Test]
    public void SomeData_PositionAtEnd_ShouldReturnEmptyMemory()
    {
        var rx = new TpsRandomAccess([1, 2, 3, 4], Encoding.ASCII);

        rx.JumpAbsolute(4);

        var mem = rx.PeekRemainingMemory();

        Assert.That(mem.IsEmpty, Is.True);
    }

    [Test]
    public void SomeData_WithBaseOffset_PositionAtEnd_ShouldReturnEmptyMemory()
    {
        var rx = new TpsRandomAccess([1, 2, 3, 4], baseOffset: 1, length: 3, Encoding.ASCII);

        rx.JumpAbsolute(3);

        var mem = rx.PeekRemainingMemory();

        Assert.That(mem.IsEmpty, Is.True);
    }

    [Test]
    public void SomeData_WithLength_PositionAtEnd_ShouldReturnEmptyMemory()
    {
        var rx = new TpsRandomAccess([1, 2, 3, 4], baseOffset: 0, length: 3, Encoding.ASCII);

        rx.JumpAbsolute(3);

        var mem = rx.PeekRemainingMemory();

        Assert.That(mem.IsEmpty, Is.True);
    }

    [Test]
    public void SomeData_WithBaseOffset_WithLength_PositionAtEnd_ShouldReturnEmptyMemory()
    {
        var rx = new TpsRandomAccess([1, 2, 3, 4], baseOffset: 0, length: 2, Encoding.ASCII);

        rx.JumpAbsolute(2);

        var mem = rx.PeekRemainingMemory();

        Assert.That(mem.IsEmpty, Is.True);
    }
}
