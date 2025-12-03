using NUnit.Framework;
using System.Text;

namespace TpsParser.Tests;

internal sealed class TestTableNameRecordPayload
{
    [Test]
    public void ShouldParse()
    {
        byte[] data = [
            0xfe,                                       /* PayloadType */
            0x55, 0x4e, 0x4e, 0x41, 0x4d, 0x45, 0x44,   /* Name */
            0x00, 0x01, 0xb3, 0x82                      /* TableNumber */
            ];

        var result = new TableNameRecordPayload { PayloadData = data, PayloadHeaderLength = 8 };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.TableNumber, Is.EqualTo(0x0001b382));

            string name = result.GetName(Encoding.ASCII);

            Assert.That(name, Is.EqualTo("UNNAMED"));
        }
    }
}
