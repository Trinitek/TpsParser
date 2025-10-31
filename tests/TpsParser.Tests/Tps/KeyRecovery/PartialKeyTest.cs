using NUnit.Framework;
using System.Threading.Tasks;
using TpsParser.Binary;
using TpsParser.Tps;
using TpsParser.Tps.KeyRecovery;

namespace TpsParser.Tests.Tps.KeyRecovery;

[TestFixture]
internal sealed class PartialKeyTest
{
    [Test]
    public void ShouldEqualsAndCompare()
    {
        var k1 = new PartialKey();
        var k2 = k1.Apply(index: 0x0F, keyPiece: 42);
        var k3 = k1.Apply(index: 0x0F, keyPiece: 42);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(k2, Is.Not.EqualTo(k1));
            Assert.That(k2, Is.EqualTo(k3));

            Assert.That(k1.CompareTo(k2), Is.EqualTo(-42));
            Assert.That(k2.CompareTo(k1), Is.EqualTo(42));
            Assert.That(k2.CompareTo(k3), Is.Zero);
        }
    }

    [Test]
    public void ShouldInvalid()
    {
        var k1 = new PartialKey();

        Assert.That(k1.GetInvalidIndexes().Count, Is.EqualTo(16));
        Assert.That(k1.GetInvalidIndexes()[0], Is.EqualTo(15));
        Assert.That(k1.IsComplete, Is.False);

        for (int i = 0; i < 16; i++)
        {
            k1 = k1.Apply(index: i, keyPiece: i);
        }

        Assert.That(k1.GetInvalidIndexes().Count, Is.Zero);
        Assert.That(k1.IsComplete);
    }

    [Test]
    public void ShouldDecrypt()
    {
        var k = new Key("aaa");

        var plain = new byte[64];
        var crypt = new byte[64];

        k.Encrypt64(new TpsRandomAccess(crypt));

        var blockPlain = new Block(new TpsRandomAccess(plain), isEncrypted: false);
        var blockCrypt = new Block(new TpsRandomAccess(crypt), isEncrypted: true);

        var pk = new PartialKey().Apply(index: 15, keyPiece: k.GetWord(15));
        var result = pk.PartialDecrypt(index: 15, block: blockCrypt);

        Assert.That(result.Values[15], Is.EqualTo(blockPlain.Values[15]));
    }

    [Test]
    public async Task ShouldKeyScan()
    {
        var k = new Key("aaa");

        var plain = new byte[64];
        var crypt = new byte[64];

        k.Encrypt64(new TpsRandomAccess(crypt));

        var blockPlain = new Block(new TpsRandomAccess(plain), isEncrypted: false);
        var blockCrypt = new Block(new TpsRandomAccess(crypt), isEncrypted: true);

        var pk = new PartialKey().Apply(index: 15, keyPiece: k.GetWord(15));

        var result = await pk.KeyIndexScan(index: 15, encryptedBlock: blockCrypt, plaintextBlock: blockPlain, cancellationToken: default);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.ContainsKey(pk));
            Assert.That(result.Count, Is.EqualTo(1216));
        }
    }
}
