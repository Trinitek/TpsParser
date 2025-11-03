using NUnit.Framework;
using System;
using System.Text;
using TpsParser.Binary;

namespace TpsParser.TypeModel.Tests;

[TestFixture]
internal sealed class TestClaDate
{
    [Test]
    public void ShouldReadFromRandomAccess()
    {
        var rx = new TpsRandomAccess([0x10, 0x07, 0xE3, 0x07], Encoding.ASCII);

        var date = rx.ReadClaDate();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(date.Year, Is.EqualTo(2019));
            Assert.That(date.Month, Is.EqualTo(7));
            Assert.That(date.Day, Is.EqualTo(16));
            Assert.That(date.ToDateTime().Value, Is.EqualTo(new DateTime(2019, 7, 16)));
        }
    }

    [Test]
    public void ShouldReadFromDateTime()
    {
        var dateTime = new DateTime(2019, 7, 16);

        var date = new ClaDate(dateTime);

        Assert.That(date.ToDateTime().Value, Is.EqualTo(dateTime));
    }
}
