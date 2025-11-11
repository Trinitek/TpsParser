using NUnit.Framework;

namespace TpsParser.Tests;

internal sealed class TestIndexRecordPayload
{
    [Test]
    public void ShouldParse_WithFields_String12_Long_Long()
    {
        byte[] data = [
            0x00, 0x02, 0xa3, 0xf8, /* TableNumber */
            0x01,                   /* DefinitionIndex */
            0x66, 0x6f, 0x6f, 0x62, 0x61, 0x72, 0x62, 0x61, 0x7a, 0x7a, 0x69, 0x70, /* Field[0] content (FSTRING) */
            0x7f, 0xfe, 0xce, 0x3b, /* Field[1] content (LONG) */
            0x7f, 0xff, 0xff, 0xff, /* Field[2] content (LONG) */
            0x00, 0x03, 0x0a, 0xe1, /* RecordNumber */
            ];

        var result = new IndexRecordPayload { PayloadData = data };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.TableNumber, Is.EqualTo(0x0002a3f8));
            Assert.That(result.DefinitionIndex, Is.EqualTo(0x01));
            Assert.That(result.RecordNumber, Is.EqualTo(0x00030ae1));
        }
    }
}
