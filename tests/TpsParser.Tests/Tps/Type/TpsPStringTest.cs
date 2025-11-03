using NUnit.Framework;
using System;
using System.Text;
using TpsParser.Binary;

namespace TpsParser.TypeModel.Tests;

[TestFixture]
internal sealed class TpsPStringTest
{
    [Test]
    public void ShouldReadFromRandomAccess()
    {
        var rx = new TpsRandomAccess([0x05, 0x48, 0x65, 0x6C, 0x6C, 0x6F], Encoding.ASCII);

        var str = rx.ReadClaPString();

        Assert.That(str.Value, Is.EqualTo("Hello"));
    }

    [Test]
    public void ShouldReadFromString()
    {
        var str = new ClaPString("Hello");

        Assert.That(str.Value, Is.EqualTo("Hello"));
    }

    [Test]
    public void ShouldThrowWhenStringCtorIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ClaPString(null));
    }
}
