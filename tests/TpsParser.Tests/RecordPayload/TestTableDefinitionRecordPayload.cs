using NUnit.Framework;

namespace TpsParser.Tests;

internal sealed class TestTableDefinitionRecordPayload
{
    [Test]
    public void ShouldParse()
    {
        byte[] data = [
            0x00, 0x01, 0xb3, 0x82, /* TableNumber */
            0xfa,                   /* PayloadType */
            0x01, 0x00,             /* SequenceNumber */
            0xde, 0xad, 0xbe, 0xef, /* Content (bogus deadbeef) */
            ];

        var result = new TableDefinitionRecordPayload { PayloadData = data };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.TableNumber, Is.EqualTo(0x0001b382));
            Assert.That(result.SequenceNumber, Is.EqualTo(1));
            Assert.That(result.Content.ToArray(), Is.EqualTo([0xde, 0xad, 0xbe, 0xef]));
        }
    }
}
