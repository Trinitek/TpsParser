using NUnit.Framework;

namespace TpsParser.Tests;

internal sealed class TestMetadataRecordPayload
{
    [Test]
    public void ShouldParse_DataContent()
    {
        byte[] data = [
            0x00, 0x02, 0xa3, 0xf8, /* TableNumber */
            0xf6,                   /* PayloadType */
            0xf3,                   /* AboutPayloadType */
            0x58, 0x1c, 0x01, 0x00, /* (DataMetadata) DataRecordCount */
            0x00, 0x00, 0x00, 0x00, /* (Unknown) */
            ];

        var result = new MetadataRecordPayload { PayloadData = data };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.TableNumber, Is.EqualTo(0x0002a3f8));
            Assert.That(result.AboutType, Is.EqualTo(RecordPayloadType.Data));
            Assert.That(result.IsAboutData, Is.True);
            Assert.That(result.IsAboutIndex, Is.False);

            Assert.That(result.TryParseContentAsIndexMetadata(out _), Is.False);
            Assert.That(result.TryParseContentAsDataMetadata(out var metadata), Is.True);

            if (metadata is null)
            {
                return;
            }

            Assert.That(metadata.DataRecordCount, Is.EqualTo(0x00011c58));
        }
    }

    [Test]
    public void ShouldParse_IndexContent()
    {
        byte[] data = [
            0x00, 0x02, 0xa3, 0xf8, /* TableNumber */
            0xf6,                   /* PayloadType */
            0x03,                   /* AboutPayloadType */
            0xb3, 0x1b, 0x01, 0x00, /* (IndexMetadata) DataRecordCount */
            0x00, 0x00, 0x00, 0x00, /* (Unknown) */
            ];

        var result = new MetadataRecordPayload { PayloadData = data };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.TableNumber, Is.EqualTo(0x0002a3f8));
            Assert.That(result.AboutType, Is.EqualTo((RecordPayloadType)3));
            Assert.That(result.IsAboutData, Is.False);
            Assert.That(result.IsAboutIndex, Is.True);

            Assert.That(result.TryParseContentAsDataMetadata(out _), Is.False);
            Assert.That(result.TryParseContentAsIndexMetadata(out var metadata), Is.True);

            if (metadata is null)
            {
                return;
            }

            Assert.That(metadata.DataRecordCount, Is.EqualTo(0x00011bb3));
        }
    }
}
