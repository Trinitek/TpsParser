using NUnit.Framework;
using System.Linq;

namespace TpsParser.Tests;

internal sealed class TestTpsBlob
{
    [Test]
    public void TpsMemoBuilder_BuildTpsBlob_OneSequence_ShouldHaveCorrectValues()
    {
        byte[] payloadData0 = [
            0x00, 0x00, 0x00, 0x01,         /* TableNumber */
            (byte)RecordPayloadType.Memo,   /* PayloadType */
            0x00, 0x00, 0x00, 0x03,         /* RecordNumber */
            0x01,                           /* DefinitionIndex */
            0x00, 0x00,                     /* SequenceNumber = 0 */
            0x05, 0x00, 0x00, 0x00,         /* BlobContent length = 5 */
            0x48, 0x65, 0x6C, 0x6C, 0x6F,   /* BlobContent = "Hello" */
            ];

        var payload0 = new MemoRecordPayload
        {
            PayloadData = payloadData0
        };

        var tpsBlobs = TpsMemoBuilder.BuildTpsBlobs([payload0]);

        var tpsBlob = tpsBlobs.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(tpsBlob.TableNumber, Is.EqualTo(1));
            Assert.That(tpsBlob.RecordNumber, Is.EqualTo(3));
            Assert.That(tpsBlob.DefinitionIndex, Is.EqualTo(1));
            Assert.That(tpsBlob.Length, Is.EqualTo(5));
            Assert.That(tpsBlob.GetBlobContentSegments().Select(m => m.ToArray()), Is.EqualTo((byte[][])[
                [0x48, 0x65, 0x6C, 0x6C, 0x6F]
                ]));
            Assert.That(tpsBlob.ToArray(), Is.EqualTo((byte[])[0x48, 0x65, 0x6C, 0x6C, 0x6F]));
        }
    }

    [Test]
    public void TpsBlob_OneSequence_ShouldHaveCorrectValues()
    {
        byte[] payloadData0 = [
            0x00, 0x00, 0x00, 0x01,         /* TableNumber */
            (byte)RecordPayloadType.Memo,   /* PayloadType */
            0x00, 0x00, 0x00, 0x03,         /* RecordNumber */
            0x01,                           /* DefinitionIndex */
            0x00, 0x00,                     /* SequenceNumber = 0 */
            0x05, 0x00, 0x00, 0x00,         /* BlobContent length = 5 */
            0x48, 0x65, 0x6C, 0x6C, 0x6F,   /* BlobContent = "Hello" */
            ];

        var payload0 = new MemoRecordPayload
        {
            PayloadData = payloadData0
        };

        var tpsBlob = new TpsBlob { MemoPayloads = [payload0] };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(tpsBlob.TableNumber, Is.EqualTo(1));
            Assert.That(tpsBlob.RecordNumber, Is.EqualTo(3));
            Assert.That(tpsBlob.DefinitionIndex, Is.EqualTo(1));
            Assert.That(tpsBlob.Length, Is.EqualTo(5));
            Assert.That(tpsBlob.GetBlobContentSegments().Select(m => m.ToArray()), Is.EqualTo((byte[][])[
                [0x48, 0x65, 0x6C, 0x6C, 0x6F]
                ]));
            Assert.That(tpsBlob.ToArray(), Is.EqualTo((byte[])[0x48, 0x65, 0x6C, 0x6C, 0x6F]));
        }
    }

    [Test]
    public void TpsMemoBuilder_BuildTpsBlob_TwoSequences_ShouldHaveCorrectValues()
    {
        byte[] payloadData0 = [
            0x00, 0x00, 0x00, 0x01,         /* TableNumber */
            (byte)RecordPayloadType.Memo,   /* PayloadType */
            0x00, 0x00, 0x00, 0x03,         /* RecordNumber */
            0x01,                           /* DefinitionIndex */
            0x00, 0x00,                     /* SequenceNumber = 0 */
            0x0b, 0x00, 0x00, 0x00,         /* BlobContent length = 5+6=11 */
            0x48, 0x65, 0x6C, 0x6C, 0x6F,   /* BlobContent = "Hello" */
            ];

        var payload0 = new MemoRecordPayload
        {
            PayloadData = payloadData0
        };

        byte[] payloadData1 = [
            0x00, 0x00, 0x00, 0x01,         /* TableNumber */
            (byte)RecordPayloadType.Memo,   /* PayloadType */
            0x00, 0x00, 0x00, 0x03,         /* RecordNumber */
            0x01,                           /* DefinitionIndex */
            0x00, 0x01,                     /* SequenceNumber = 1 */
                                            /* BlobContent length not set */
            0x20, 0x57, 0x6f, 0x72, 0x6c, 0x64  /* BlobContent = " World" */
            ];

        var payload1 = new MemoRecordPayload
        {
            PayloadData = payloadData1
        };

        var tpsBlobs = TpsMemoBuilder.BuildTpsBlobs([payload0, payload1]);

        var tpsBlob = tpsBlobs.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(tpsBlob.TableNumber, Is.EqualTo(1));
            Assert.That(tpsBlob.RecordNumber, Is.EqualTo(3));
            Assert.That(tpsBlob.DefinitionIndex, Is.EqualTo(1));
            Assert.That(tpsBlob.Length, Is.EqualTo(11));
            Assert.That(tpsBlob.GetBlobContentSegments().Select(m => m.ToArray()), Is.EqualTo((byte[][])[
                [0x48, 0x65, 0x6C, 0x6C, 0x6F],
                [0x20, 0x57, 0x6f, 0x72, 0x6c, 0x64]
                ]));
            Assert.That(tpsBlob.ToArray(), Is.EqualTo((byte[])[
                0x48, 0x65, 0x6C, 0x6C, 0x6F,
                0x20, 0x57, 0x6f, 0x72, 0x6c, 0x64
                ]));
        }
    }

    [Test]
    public void TpsBlob_TwoSequences_ShouldHaveCorrectValues()
    {
        byte[] payloadData0 = [
            0x00, 0x00, 0x00, 0x01,         /* TableNumber */
            (byte)RecordPayloadType.Memo,   /* PayloadType */
            0x00, 0x00, 0x00, 0x03,         /* RecordNumber */
            0x01,                           /* DefinitionIndex */
            0x00, 0x00,                     /* SequenceNumber = 0 */
            0x0b, 0x00, 0x00, 0x00,         /* BlobContent length = 5+6=11 */
            0x48, 0x65, 0x6C, 0x6C, 0x6F,   /* BlobContent = "Hello" */
            ];

        var payload0 = new MemoRecordPayload
        {
            PayloadData = payloadData0
        };

        byte[] payloadData1 = [
            0x00, 0x00, 0x00, 0x01,         /* TableNumber */
            (byte)RecordPayloadType.Memo,   /* PayloadType */
            0x00, 0x00, 0x00, 0x03,         /* RecordNumber */
            0x01,                           /* DefinitionIndex */
            0x00, 0x01,                     /* SequenceNumber = 1 */
                                            /* BlobContent length not set */
            0x20, 0x57, 0x6f, 0x72, 0x6c, 0x64  /* BlobContent = " World" */
            ];

        var payload1 = new MemoRecordPayload
        {
            PayloadData = payloadData1
        };

        var tpsBlob = new TpsBlob { MemoPayloads = [payload0, payload1] };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(tpsBlob.TableNumber, Is.EqualTo(1));
            Assert.That(tpsBlob.RecordNumber, Is.EqualTo(3));
            Assert.That(tpsBlob.DefinitionIndex, Is.EqualTo(1));
            Assert.That(tpsBlob.Length, Is.EqualTo(11));
            Assert.That(tpsBlob.GetBlobContentSegments().Select(m => m.ToArray()), Is.EqualTo((byte[][])[
                [0x48, 0x65, 0x6C, 0x6C, 0x6F],
                [0x20, 0x57, 0x6f, 0x72, 0x6c, 0x64]
                ]));
            Assert.That(tpsBlob.ToArray(), Is.EqualTo((byte[])[
                0x48, 0x65, 0x6C, 0x6C, 0x6F,
                0x20, 0x57, 0x6f, 0x72, 0x6c, 0x64
                ]));
        }
    }
}
