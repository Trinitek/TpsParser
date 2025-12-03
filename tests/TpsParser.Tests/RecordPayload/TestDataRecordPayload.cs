using NUnit.Framework;

namespace TpsParser.Tests;

internal sealed class TestDataRecordPayload
{
    [Test]
    public void ShouldParse()
    {
        byte[] data = [
            0x00, 0x01, 0xb3, 0x82, /* TableNumber */
            0xf3,                   /* PayloadType */
            0x00, 0x01, 0xb3, 0x86, /* RecordNumber */
            0xde, 0xad, 0xbe, 0xef, /* Content (bogus deadbeef) */
            ];

        var result = new DataRecordPayload { PayloadData = data };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.TableNumber, Is.EqualTo(0x0001b382));
            Assert.That(result.RecordNumber, Is.EqualTo(0x0001b386));
            Assert.That(result.Content.ToArray(), Is.EqualTo([0xde, 0xad, 0xbe, 0xef]));
        }
    }
}
