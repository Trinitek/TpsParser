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

    [Test]
    public void Equality()
    {
        var s_str1 = new ClaFString("Foo ");
        var s_str2 = new ClaFString("Foo ");
        var s_mem1 = new ClaFString("Foo "u8.ToArray());
        var s_mem2 = new ClaFString("Foo "u8.ToArray());

        using (Assert.EnterMultipleScope())
        {
#pragma warning disable CS1718 // Comparison made to same variable
#pragma warning disable NUnit2010 // Use EqualConstraint for better assertion messages in case of failure

            Assert.That(s_str1 == s_str1);
            Assert.That(s_str1 == s_str2);

            Assert.That(s_str2 == s_str1);
            Assert.That(s_str2 == s_str2);

            Assert.That(s_mem1 == s_mem1);
            Assert.That(s_mem1 == s_mem2);

            Assert.That(s_mem2 == s_mem1);
            Assert.That(s_mem2 == s_mem2);

#pragma warning restore NUnit2010 // Use EqualConstraint for better assertion messages in case of failure
#pragma warning restore CS1718 // Comparison made to same variable
        }
    }

    [Test]
    public void Inequality_StringsNotEquatableToMemory()
    {
        var s_str1 = new ClaFString("Foo ");
        var s_str2 = new ClaFString("Foo ");
        var s_mem1 = new ClaFString("Foo "u8.ToArray());
        var s_mem2 = new ClaFString("Foo "u8.ToArray());

        using (Assert.EnterMultipleScope())
        {
#pragma warning disable CS1718 // Comparison made to same variable
#pragma warning disable NUnit2010 // Use EqualConstraint for better assertion messages in case of failure

            Assert.That(s_str1 != s_mem1);
            Assert.That(s_str1 != s_mem2);

            Assert.That(s_str2 != s_mem1);
            Assert.That(s_str2 != s_mem2);

            Assert.That(s_mem1 != s_str1);
            Assert.That(s_mem1 != s_str2);

            Assert.That(s_mem2 != s_str1);
            Assert.That(s_mem2 != s_str2);

#pragma warning restore NUnit2010 // Use EqualConstraint for better assertion messages in case of failure
#pragma warning restore CS1718 // Comparison made to same variable
        }
    }

    [Test]
    public void Inequality()
    {
        var s_str1 = new ClaFString("Foo1 ");
        var s_str2 = new ClaFString("Foo2 ");
        var s_mem1 = new ClaFString("Foo3 "u8.ToArray());
        var s_mem2 = new ClaFString("Foo4 "u8.ToArray());

        using (Assert.EnterMultipleScope())
        {
#pragma warning disable CS1718 // Comparison made to same variable
#pragma warning disable NUnit2010 // Use EqualConstraint for better assertion messages in case of failure

            Assert.That(s_str1 != s_str2);
            Assert.That(s_str1 != s_mem1);
            Assert.That(s_str1 != s_mem2);

            Assert.That(s_str2 != s_str1);
            Assert.That(s_str2 != s_mem1);
            Assert.That(s_str2 != s_mem2);

            Assert.That(s_mem1 != s_str1);
            Assert.That(s_mem1 != s_str2);
            Assert.That(s_mem1 != s_mem2);

            Assert.That(s_mem2 != s_str1);
            Assert.That(s_mem2 != s_str2);
            Assert.That(s_mem2 != s_mem1);


#pragma warning restore NUnit2010 // Use EqualConstraint for better assertion messages in case of failure
#pragma warning restore CS1718 // Comparison made to same variable
        }
    }
}
