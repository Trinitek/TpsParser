using NUnit.Framework;
using TpsParser.Tps.KeyRecovery;

namespace TpsParser.Tests.KeyRecovery
{
    [TestFixture]
    public class BlockTest
    {
        [Test]
        public void ShouldCreate()
        {
            var bA = new Block(offset: 0, values: new int[16], isEncrypted: true);
            var bB = new Block(offset: 10, bA);
            var bC = new Block(block: bA);
            var bD = bA.Apply(a: 0, b: 1, va: 1, vb: 2);

            Assert.AreEqual(0, bA.Offset);
            Assert.AreEqual(10, bB.Offset);
            Assert.AreEqual(0, bC.Offset);
            Assert.AreEqual(0, bD.Offset);

            Assert.True(bA.IsEncrypted);
            Assert.True(bB.IsEncrypted);
            Assert.True(bC.IsEncrypted);
            Assert.True(bD.IsEncrypted);

            Assert.AreEqual(bA, bC, "A == C");
            Assert.AreEqual(bC, bA, "C == A");
            Assert.AreNotEqual(bB, bA, "B != A");
            Assert.AreNotEqual(bD, bA, "D != A");

            Assert.AreEqual(0, bA.Values[0]);
            Assert.AreEqual(1, bD.Values[0]);
            Assert.AreEqual(2, bD.Values[1]);

            Assert.AreEqual(-1, bA.CompareTo(bD));
            Assert.AreEqual(1, bD.CompareTo(bA));
        }
    }
}
