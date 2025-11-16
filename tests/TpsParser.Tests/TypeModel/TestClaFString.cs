using NUnit.Framework;
using System;
using System.Text;

namespace TpsParser.TypeModel.Tests;

[TestFixture]
internal sealed class TestClaFString
{
    [Test]
    public void ShouldReadFromRandomAccess()
    {
        var rx = new TpsRandomAccess([0x48, 0x65, 0x6C, 0x6C, 0x6F], Encoding.ASCII);

        var str = rx.ReadClaFString();

        Assert.That(str.ToString(), Is.EqualTo("Hello"));
    }

    [Test]
    public void ShouldReadFromString()
    {
        var str = new ClaFString("Hello");

        Assert.That(str.ToString(), Is.EqualTo("Hello"));
    }

    [Test]
    public void ShouldThrowWhenStringCtorIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ClaFString((string)null!));
    }
}
