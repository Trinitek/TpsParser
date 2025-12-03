using NUnit.Framework;
using System.IO;
using System.Linq;

namespace TpsParser.Tests.ResourceTests;

internal sealed class TestTableWithMemos
{
    private const string Filename = "Resources/table-with-memos.tps";

    [Test]
    public void ShouldHaveFileHeader()
    {
        using var fs = new FileStream(Filename, FileMode.Open);

        var file = new TpsFile(fs);

        var header = file.GetFileHeader();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(header.Address, Is.Zero);
            Assert.That(header.HeaderSize, Is.EqualTo(512));
            Assert.That(header.FileLength1, Is.EqualTo(1792));
            Assert.That(header.FileLength2, Is.EqualTo(1792));
            Assert.That(header.MagicNumber, Is.EqualTo("tOpS"));
            Assert.That(header.Zeroes, Is.Zero);
            Assert.That(header.LastIssuedRow, Is.EqualTo(5));
            Assert.That(header.Changes, Is.EqualTo(11));
            Assert.That(header.ManagementBlockOffset, Is.EqualTo(512));
            Assert.That(header.BlockDescriptors, Is.EqualTo(new TpsBlockDescriptor[] {
                new(512, 512),   new(512, 512),   new(512, 512),   new(512, 512),   new(512, 512),   new(512, 1792),  new(1792, 1792), new(1792, 1792),
                new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792),
                new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792),
                new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792),
                new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792),
                new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792),
                new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792),
                new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792),
            }));

            Assert.That(header, Is.EqualTo(new TpsFileHeader
            {
                Address = 0,
                HeaderSize = 512,
                FileLength1 = 1792,
                FileLength2 = 1792,
                MagicNumber = TpsFileHeader.TopSpeedMagicNumber,
                Zeroes = 0,
                LastIssuedRow = 5,
                Changes = 11,
                ManagementBlockOffset = 512,
                BlockDescriptors = [
                    new(512, 512),   new(512, 512),   new(512, 512),   new(512, 512),   new(512, 512),   new(512, 1792),  new(1792, 1792), new(1792, 1792),
                    new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792),
                    new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792),
                    new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792),
                    new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792),
                    new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792),
                    new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792),
                    new(1792, 1792), new(1792, 1792), new(1792, 1792), new(1792, 1792),
                    ]
            }));
        }
    }

    [Test]
    public void ShouldHavePages()
    {
        using var fs = new FileStream(Filename, FileMode.Open);

        var file = new TpsFile(fs);

        var blocks = file.GetBlocks().ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(blocks, Has.Count.EqualTo(1));

            var b0 = blocks[0];
            var b0pages = b0.GetPages();

            Assert.That(b0pages, Has.Count.EqualTo(1));

            var p0 = b0pages[0];

            Assert.That(p0.AbsoluteAddress, Is.EqualTo(512));
            Assert.That(p0.Size, Is.EqualTo(1123));
            Assert.That(p0.SizeUncompressed, Is.EqualTo(1804));
            Assert.That(p0.SizeUncompressedExpanded, Is.EqualTo(1898));
            Assert.That(p0.RecordCount, Is.EqualTo(14));
            Assert.That(p0.Flags, Is.Zero);
        }
    }

    [Test]
    public void ShouldHaveTableDefinitionRecords()
    {
        using var fs = new FileStream(Filename, FileMode.Open);

        var file = new TpsFile(fs);

        var tableDefinitions = file.GetTableDefinitions(errorHandlingOptions: ErrorHandlingOptions.Strict);

        Assert.That(tableDefinitions, Has.Count.EqualTo(1));

        var def = tableDefinitions[1];

        using (Assert.EnterMultipleScope())
        {
            Assert.That(def.Fields, Has.Length.EqualTo(2));
            Assert.That(def.Memos, Has.Length.EqualTo(2));
            Assert.That(def.Indexes, Has.Length.EqualTo(0));

            var f0 = def.Fields[0];

            Assert.That(f0.BcdDigitsAfterDecimalPoint, Is.Zero);
            Assert.That(f0.BcdElementLength, Is.Zero);
            Assert.That(f0.ElementCount, Is.EqualTo(1));
            Assert.That(f0.Flags, Is.Zero);
            Assert.That(f0.FullName, Is.EqualTo("FIR:Name"));
            Assert.That(f0.Index, Is.Zero);
            Assert.That(f0.Length, Is.EqualTo(64));
            Assert.That(f0.StringLength, Is.EqualTo(64));
            Assert.That(f0.StringMask, Is.EqualTo(string.Empty));
            Assert.That(f0.TypeCode, Is.EqualTo(FieldTypeCode.FString));

            var f1 = def.Fields[1];

            Assert.That(f1.BcdDigitsAfterDecimalPoint, Is.Zero);
            Assert.That(f1.BcdElementLength, Is.Zero);
            Assert.That(f1.ElementCount, Is.EqualTo(1));
            Assert.That(f1.Flags, Is.Zero);
            Assert.That(f1.FullName, Is.EqualTo("FIR:Date"));
            Assert.That(f1.Index, Is.EqualTo(1));
            Assert.That(f1.Length, Is.EqualTo(4));
            Assert.That(f1.StringLength, Is.Zero);
            Assert.That(f1.StringMask, Is.EqualTo(string.Empty));
            Assert.That(f1.TypeCode, Is.EqualTo(FieldTypeCode.Date));

            var m0 = def.Memos[0];

            Assert.That(m0.ExternalFileName, Is.Empty);
            Assert.That(m0.FullName, Is.EqualTo("FIR:AdditionalNotes"));
            Assert.That(m0.Flags, Is.Zero);
            Assert.That(m0.Length, Is.EqualTo(2048));

            var m1 = def.Memos[1];

            Assert.That(m1.ExternalFileName, Is.Empty);
            Assert.That(m1.FullName, Is.EqualTo("FIR:Notes"));
            Assert.That(m1.Flags, Is.Zero);
            Assert.That(m1.Length, Is.EqualTo(1024));
        }
    }
}
