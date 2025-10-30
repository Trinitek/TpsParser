using NUnit.Framework;
using System.Globalization;
using System.IO;
using System.Linq;
using TpsParser.Binary;
using TpsParser.Tps;

namespace TpsParser.Tests.Tps
{
    [TestFixture]
    public class KeyTest
    {
        private static readonly string EncryptedHeader =
            "BC DC 5C 92 90 BC DF B8 B0 5B AF BB A5 F8 30 C5 " +
            "05 AE FF D0 F0 BF F7 C2 E0 DC FC 57 F7 BF FB 93 " +
            "A8 54 DA C0 70 6D AD AA 30 E9 BD FA D0 7A FD D4 " +
            "DD FF FE E1 50 F9 FE C1 E0 D3 77 E3 F5 7A BF F1";

        private static readonly string DecryptedHeader =
            "00 00 00 00 00 02 00 c2 05 00 00 c2 05 00 74 4f " +
            "70 53 00 00 00 00 1a 25 07 00 00 00 05 00 00 00 " +
            "00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 " +
            "00 00 00 00 00 00 00 00 05 00 00 00 0c 00 00 00";

        private byte[] ParseHex(string hexString) =>
            hexString.Split(' ')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => byte.Parse(s, NumberStyles.HexNumber))
                .ToArray();

        [Test]
        public void ShouldCreate()
        {
            var key = new Key("a");

            Assert.AreEqual((uint)0x7052a480, (uint)key.GetWord(0));
            Assert.AreEqual((uint)0x68dd1890, (uint)key.GetWord(1));
            Assert.AreEqual((uint)0xf1ab48a0, (uint)key.GetWord(2));
            Assert.AreEqual((uint)0x48dcf8a0, (uint)key.GetWord(3));
        }

        [Test]
        public void ShouldDecryptBlock()
        {
            var rx = new TpsRandomAccess(ParseHex(EncryptedHeader));
            var key = new Key("a");

            key.Decrypt64(rx);

            var expectedDecrypted = ParseHex(DecryptedHeader);
            var actualDecrypted = rx.GetData();

            CollectionAssert.AreEqual(expectedDecrypted, actualDecrypted);
        }

        [Test]
        public void ShouldEncryptBlock()
        {
            var rx = new TpsRandomAccess(ParseHex(DecryptedHeader));
            var key = new Key("a");

            key.Encrypt64(rx);

            var expectedEncrypted = ParseHex(EncryptedHeader);
            var actualEncrypted = rx.GetData();

            CollectionAssert.AreEqual(expectedEncrypted, actualEncrypted);
        }

        [Test]
        public void ShouldDecryptFile()
        {
            using (var fsEncrypted = new FileStream("Resources/encrypted-a.tps", FileMode.Open))
            using (var fsUnencrypted = new FileStream("Resources/not-encrypted.tps", FileMode.Open))
            {
                var encryptedFile = new RandomAccessTpsFile(fsEncrypted, new Key("a"));
                var decryptedFile = new RandomAccessTpsFile(fsUnencrypted);

                var encryptedDefinitions = encryptedFile.GetTableDefinitions(ignoreErrors: false);
                var decryptedDefinitions = decryptedFile.GetTableDefinitions(ignoreErrors: false);

                Assert.AreEqual(decryptedDefinitions.Count, encryptedDefinitions.Count);

                // Note that record IDs may differ.
                var encryptedRecords = encryptedFile.GetDataRecords(table: 2, tableDefinition: encryptedDefinitions[2], ignoreErrors: false);
                var decryptedRecords = decryptedFile.GetDataRecords(table: 1, tableDefinition: decryptedDefinitions[1], ignoreErrors: false);

                Assert.AreEqual(decryptedRecords.Count(), encryptedRecords.Count());

                var zip = decryptedRecords.Zip(encryptedRecords, (d, e) => (dec: d, enc: e));

                foreach (var (dec, enc) in zip)
                {
                    CollectionAssert.AreEqual(dec.Values, enc.Values);
                }
            }
        }

        [Test]
        public void ShouldFailToReadEncryptedFileWithoutPassword()
        {
            using (var fsEncrypted = new FileStream("Resources/encrypted-a.tps", FileMode.Open))
            {
                var encryptedFile = new RandomAccessTpsFile(fsEncrypted);

                Assert.Throws<NotATopSpeedFileException>(() => encryptedFile.GetHeader());
            }
        }
    }
}
