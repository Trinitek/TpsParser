using NUnit.Framework;
using System.Text;

namespace TpsParser.Tests;

internal sealed class TestTpsRecord
{
    [Test]
    public void ShouldParse_OwnsHeaderLength_OwnsTotalLength()
    {
        byte[] data = [
            0xc0,       /* Flags */
            0x13, 0x00, /* PayloadTotalLength */
            0x0f, 0x00, /* PayloadHeaderLength */
                        /* Payload */
            0x00, 0x00, 0x00, 0x01, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x31, 0x00,
            0x00, 0x00, 0x02
            ];

        var rx = new TpsRandomAccess(data, Encoding.ASCII);

        var result = TpsRecord.Parse(rx);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.OwnsPayloadHeaderLength, Is.True);
            Assert.That(result.OwnsPayloadTotalLength, Is.True);
            Assert.That(result.PayloadInheritedBytes, Is.Zero);
            Assert.That(result.PayloadTotalLength, Is.EqualTo(0x13));
            Assert.That(result.PayloadHeaderLength, Is.EqualTo(0x0f));
            Assert.That(result.RecordData.ToArray(), Is.EqualTo(data));
            Assert.That(result.PayloadData.ToArray(), Is.EqualTo([
                0x00, 0x00, 0x00, 0x01, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x31, 0x00,
                0x00, 0x00, 0x02
                ]));
        }
    }

    [Test]
    public void ShouldParse_FromPrevious_OwnsHeaderLength_OwnsTotalLength_NoCopy()
    {
        byte[] d1 = [
            0xc0,       /* Flags */
            0x13, 0x00, /* PayloadTotalLength */
            0x0f, 0x00, /* PayloadHeaderLength */
                        /* Payload */
            0x00, 0x00, 0x00, 0x01, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x31, 0x00,
            0x00, 0x00, 0x02
            ];

        byte[] d2 = [
            0xc0,       /* Flags */
            0x15, 0x00, /* PayloadTotalLength */
            0x0d, 0x00, /* PayloadHeaderLength */
                        /* Payload */
            0xde, 0xad, 0xbe, 0xef, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x31, 0x00,
            0x00, 0x00, 0xaa, 0x21, 0x22
            ];

        byte[] data = [.. d1, .. d2];

        var rx = new TpsRandomAccess(data, Encoding.ASCII);

        var record1 = TpsRecord.Parse(rx);

        var result = TpsRecord.Parse(previous: record1, rx);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.OwnsPayloadHeaderLength, Is.True);
            Assert.That(result.OwnsPayloadTotalLength, Is.True);
            Assert.That(result.PayloadInheritedBytes, Is.Zero);
            Assert.That(result.PayloadTotalLength, Is.EqualTo(0x15));
            Assert.That(result.PayloadHeaderLength, Is.EqualTo(0x0d));
            Assert.That(result.RecordData.ToArray(), Is.EqualTo(d2));
            Assert.That(result.PayloadData.ToArray(), Is.EqualTo([
                0xde, 0xad, 0xbe, 0xef, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x31, 0x00,
                0x00, 0x00, 0xaa, 0x21, 0x22
                ]));
        }
    }

    [Test]
    public void ShouldParse_FromPrevious_InheritsHeaderLength_OwnsTotalLength_NoCopy()
    {
        byte[] d1 = [
            0xc0,       /* Flags */
            0x13, 0x00, /* PayloadTotalLength */
            0x0f, 0x00, /* PayloadHeaderLength */
                        /* Payload */
            0x00, 0x00, 0x00, 0x01, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x31, 0x00,
            0x00, 0x00, 0x02
            ];

        byte[] d2 = [
            0x80,       /* Flags */
            0x15, 0x00, /* PayloadTotalLength */
                        /* ... */
                        /* Payload */
            0xde, 0xad, 0xbe, 0xef, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x31, 0x00,
            0x00, 0x00, 0xaa, 0x21, 0x22
            ];

        byte[] data = [.. d1, .. d2];

        var rx = new TpsRandomAccess(data, Encoding.ASCII);

        var record1 = TpsRecord.Parse(rx);

        var result = TpsRecord.Parse(previous: record1, rx);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.OwnsPayloadHeaderLength, Is.False);
            Assert.That(result.OwnsPayloadTotalLength, Is.True);
            Assert.That(result.PayloadInheritedBytes, Is.Zero);
            Assert.That(result.PayloadTotalLength, Is.EqualTo(0x15));
            Assert.That(result.PayloadHeaderLength, Is.EqualTo(0x0f));
            Assert.That(result.RecordData.ToArray(), Is.EqualTo(d2));
            Assert.That(result.PayloadData.ToArray(), Is.EqualTo([
                0xde, 0xad, 0xbe, 0xef, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x31, 0x00,
                0x00, 0x00, 0xaa, 0x21, 0x22
                ]));
        }
    }

    [Test]
    public void ShouldParse_FromPrevious_OwnsHeaderLength_InheritsTotalLength_NoCopy()
    {
        byte[] d1 = [
            0xc0,       /* Flags */
            0x13, 0x00, /* PayloadTotalLength */
            0x0f, 0x00, /* PayloadHeaderLength */
                        /* Payload */
            0x00, 0x00, 0x00, 0x01, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x31, 0x00,
            0x00, 0x00, 0x02
            ];

        byte[] d2 = [
            0x40,       /* Flags */
                        /* ... */
            0x11, 0x00, /* PayloadHeaderLength */
                        /* Payload */
            0xde, 0xad, 0xbe, 0xef, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x31, 0x00,
            0x00, 0x00, 0xaa
            ];

        byte[] data = [.. d1, .. d2];

        var rx = new TpsRandomAccess(data, Encoding.ASCII);

        var record1 = TpsRecord.Parse(rx);

        var result = TpsRecord.Parse(previous: record1, rx);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.OwnsPayloadHeaderLength, Is.True);
            Assert.That(result.OwnsPayloadTotalLength, Is.False);
            Assert.That(result.PayloadInheritedBytes, Is.Zero);
            Assert.That(result.PayloadTotalLength, Is.EqualTo(0x13));
            Assert.That(result.PayloadHeaderLength, Is.EqualTo(0x11));
            Assert.That(result.RecordData.ToArray(), Is.EqualTo(d2));
            Assert.That(result.PayloadData.ToArray(), Is.EqualTo([
                0xde, 0xad, 0xbe, 0xef, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x31, 0x00,
                0x00, 0x00, 0xaa
                ]));
        }
    }

    [Test]
    public void ShouldParse_FromPrevious_InheritsHeaderLength_InheritsTotalLength_NoCopy()
    {
        byte[] d1 = [
            0xc0,       /* Flags */
            0x13, 0x00, /* PayloadTotalLength */
            0x0f, 0x00, /* PayloadHeaderLength */
                        /* Payload */
            0x00, 0x00, 0x00, 0x01, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x31, 0x00,
            0x00, 0x00, 0x02
            ];

        byte[] d2 = [
            0x00,       /* Flags */
                        /* ... */
                        /* ... */
                        /* Payload */
            0xde, 0xad, 0xbe, 0xef, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x31, 0x00,
            0x00, 0x00, 0xaa
            ];

        byte[] data = [.. d1, .. d2];

        var rx = new TpsRandomAccess(data, Encoding.ASCII);

        var record1 = TpsRecord.Parse(rx);

        var result = TpsRecord.Parse(previous: record1, rx);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.OwnsPayloadHeaderLength, Is.False);
            Assert.That(result.OwnsPayloadTotalLength, Is.False);
            Assert.That(result.PayloadTotalLength, Is.EqualTo(0x13));
            Assert.That(result.PayloadHeaderLength, Is.EqualTo(0x0f));
            Assert.That(result.PayloadInheritedBytes, Is.Zero);
            Assert.That(result.RecordData.ToArray(), Is.EqualTo(d2));
            Assert.That(result.PayloadData.ToArray(), Is.EqualTo([
                0xde, 0xad, 0xbe, 0xef, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x31, 0x00,
                0x00, 0x00, 0xaa
                ]));
        }
    }

    [Test]
    public void ShouldParse_FromPrevious_OwnsHeaderLength_OwnsTotalLength_Copy()
    {
        byte[] d1 = [
            0xc0,       /* Flags */
            0x13, 0x00, /* PayloadTotalLength */
            0x0f, 0x00, /* PayloadHeaderLength */
                        /* Payload */
            0xde, 0xad, 0xbe, 0xef, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x31, 0x00,
            0x00, 0x00, 0x02
            ];

        byte[] d2 = [
            0xc4,       /* Flags */
            0x09, 0x00, /* PayloadTotalLength */
            0x03, 0x00, /* PayloadHeaderLength */
                        /* Payload */
            0x32, 0x00, 0x00, 0x00, 0x03
            ];

        byte[] data = [.. d1, .. d2];

        var rx = new TpsRandomAccess(data, Encoding.ASCII);

        var record1 = TpsRecord.Parse(rx);

        var result = TpsRecord.Parse(previous: record1, rx);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.OwnsPayloadHeaderLength, Is.True);
            Assert.That(result.OwnsPayloadTotalLength, Is.True);
            Assert.That(result.PayloadTotalLength, Is.EqualTo(0x09));
            Assert.That(result.PayloadHeaderLength, Is.EqualTo(0x03));
            Assert.That(result.PayloadInheritedBytes, Is.EqualTo(0x04));
            Assert.That(result.RecordData.ToArray(), Is.EqualTo(d2));
            Assert.That(result.PayloadData.ToArray(), Is.EqualTo([
                0xde, 0xad, 0xbe, 0xef, 0x32, 0x00, 0x00, 0x00, 0x03
                ]));
        }
    }

    [Test]
    public void ShouldParse_FromPrevious_InheritsHeaderLength_OwnsTotalLength_Copy()
    {
        byte[] d1 = [
            0xc0,       /* Flags */
            0x13, 0x00, /* PayloadTotalLength */
            0x0f, 0x00, /* PayloadHeaderLength */
                        /* Payload */
            0xde, 0xad, 0xbe, 0xef, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x31, 0x00,
            0x00, 0x00, 0x02
            ];

        byte[] d2 = [
            0x8e,       /* Flags */
            0x13, 0x00, /* PayloadTotalLength */
                        /* ... */
                        /* Payload */
            0x32, 0x00, 0x00, 0x00, 0x03
            ];

        byte[] data = [.. d1, .. d2];

        var rx = new TpsRandomAccess(data, Encoding.ASCII);

        var record1 = TpsRecord.Parse(rx);

        var result = TpsRecord.Parse(previous: record1, rx);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.OwnsPayloadHeaderLength, Is.False);
            Assert.That(result.OwnsPayloadTotalLength, Is.True);
            Assert.That(result.PayloadTotalLength, Is.EqualTo(0x13));
            Assert.That(result.PayloadHeaderLength, Is.EqualTo(0x0f));
            Assert.That(result.PayloadInheritedBytes, Is.EqualTo(0x0e));
            Assert.That(result.RecordData.ToArray(), Is.EqualTo(d2));
            Assert.That(result.PayloadData.ToArray(), Is.EqualTo([
                0xde, 0xad, 0xbe, 0xef, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30,
                0x32, 0x00, 0x00, 0x00, 0x03
                ]));
        }
    }

    [Test]
    public void ShouldParse_FromPrevious_OwnsHeaderLength_InheritsTotalLength_Copy()
    {
        byte[] d1 = [
            0xc0,       /* Flags */
            0x13, 0x00, /* PayloadTotalLength */
            0x0f, 0x00, /* PayloadHeaderLength */
                        /* Payload */
            0xde, 0xad, 0xbe, 0xef, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x31, 0x00,
            0x00, 0x00, 0x02
            ];

        byte[] d2 = [
            0x4e,       /* Flags */
                        /* ... */
            0x09, 0x00, /* PayloadHeaderLength */
                        /* Payload */
            0x32, 0x00, 0x00, 0x00, 0x03
            ];

        byte[] data = [.. d1, .. d2];

        var rx = new TpsRandomAccess(data, Encoding.ASCII);

        var record1 = TpsRecord.Parse(rx);

        var result = TpsRecord.Parse(previous: record1, rx);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.OwnsPayloadHeaderLength, Is.True);
            Assert.That(result.OwnsPayloadTotalLength, Is.False);
            Assert.That(result.PayloadTotalLength, Is.EqualTo(0x13));
            Assert.That(result.PayloadHeaderLength, Is.EqualTo(0x09));
            Assert.That(result.PayloadInheritedBytes, Is.EqualTo(0x0e));
            Assert.That(result.RecordData.ToArray(), Is.EqualTo(d2));
            Assert.That(result.PayloadData.ToArray(), Is.EqualTo([
                0xde, 0xad, 0xbe, 0xef, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30,
                0x32, 0x00, 0x00, 0x00, 0x03
                ]));
        }
    }

    [Test]
    public void ShouldParse_FromPrevious_InheritsHeaderLength_InheritsTotalLength_Copy()
    {
        byte[] d1 = [
            0xc0,       /* Flags */
            0x13, 0x00, /* PayloadTotalLength */
            0x0f, 0x00, /* PayloadHeaderLength */
                        /* Payload */
            0x00, 0x00, 0x00, 0x01, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x31, 0x00,
            0x00, 0x00, 0x02
            ];

        byte[] d2 = [
            0x0e,       /* Flags */
                        /* ... */
                        /* ... */
                        /* Payload */
            0x32, 0x00, 0x00, 0x00, 0x03
            ];

        byte[] data = [.. d1, .. d2];

        var rx = new TpsRandomAccess(data, Encoding.ASCII);

        var record1 = TpsRecord.Parse(rx);

        var result = TpsRecord.Parse(previous: record1, rx);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.OwnsPayloadHeaderLength, Is.False);
            Assert.That(result.OwnsPayloadTotalLength, Is.False);
            Assert.That(result.PayloadTotalLength, Is.EqualTo(0x13));
            Assert.That(result.PayloadHeaderLength, Is.EqualTo(0x0f));
            Assert.That(result.PayloadInheritedBytes, Is.EqualTo(0x0e));
            Assert.That(result.RecordData.ToArray(), Is.EqualTo(d2));
            Assert.That(result.PayloadData.ToArray(), Is.EqualTo([
                0x00, 0x00, 0x00, 0x01, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x32, 0x00,
                0x00, 0x00, 0x03
                ]));
        }
    }
}
