using NUnit.Framework;
using TpsParser.Tps.KeyRecovery;

namespace TpsParser.Tests.KeyRecovery
{
    [TestFixture]
    public class PartialKeyTest
    {
        [Test]
        public void ShouldEqualsAndCompare()
        {
            var k1 = new PartialKey();
            var k2 = k1.Apply(index: 0x0F, keyA: 42);
            var k3 = k1.Apply(index: 0x0F, keyA: 42);

            Assert.AreNotEqual(k1, k2);
            Assert.AreEqual(k3, k2);

            Assert.AreEqual(-42, k1.CompareTo(k2));
            Assert.AreEqual(42, k2.CompareTo(k1));
            Assert.AreEqual(0, k2.CompareTo(k3));
        }
    }
}
