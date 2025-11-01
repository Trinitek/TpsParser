using NUnit.Framework;
using System;
using System.Text;
using TpsParser.Binary;

namespace TpsParser.TypeModel.Tests;

[TestFixture]
internal sealed class TpsCStringTest
{
    [Test]
    public void ShouldReadFromRandomAccess()
    {
        var rx = new TpsRandomAccess([0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x00], Encoding.ASCII);

        var str = rx.ReadTpsCString();

        Assert.That(str.Value, Is.EqualTo("Hello"));
    }

    [Test]
    public void ShouldReadFromString()
    {
        var str = new TpsCString("Hello");

        Assert.That(str.Value, Is.EqualTo("Hello"));
    }

    [Test]
    public void ShouldThrowWhenStringCtorIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new TpsCString(null));
    }
}
