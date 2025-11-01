using NUnit.Framework;
using System;
using System.Text;
using TpsParser.Binary;

namespace TpsParser.TypeModel.Tests;

[TestFixture]
internal sealed class TpsStringTest
{
    [Test]
    public void ShouldReadFromRandomAccess()
    {
        var rx = new TpsRandomAccess([0x48, 0x65, 0x6C, 0x6C, 0x6F], Encoding.ASCII);

        var str = rx.ReadTpsString();

        Assert.That(str.Value, Is.EqualTo("Hello"));
    }

    [Test]
    public void ShouldReadFromString()
    {
        var str = new TpsString("Hello");

        Assert.That(str.Value, Is.EqualTo("Hello"));
    }

    [Test]
    public void ShouldThrowWhenStringCtorIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new TpsString(null));
    }
}
