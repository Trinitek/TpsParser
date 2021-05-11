﻿using NUnit.Framework;
using System;
using System.Text;
using TpsParser.Tps.Type;

namespace TpsParser.Tests.Tps.Type
{
    [TestFixture]
    public class TpsPStringTest
    {
        [Test]
        public void ShouldReadFromRandomAccess()
        {
            var rx = new TpsReader(new byte[] { 0x05, 0x48, 0x65, 0x6C, 0x6C, 0x6F });

            var str = rx.ReadTpsPString(Encoding.ASCII);

            Assert.AreEqual("Hello", str.Value);
        }

        [Test]
        public void ShouldReadFromString()
        {
            var str = new TpsPString("Hello");

            Assert.AreEqual("Hello", str.Value);
        }

        [Test]
        public void ShouldThrowWhenStringCtorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new TpsPString(null));
        }
    }
}
