using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TpsParser.Tests.ResourceTests;

internal sealed class TestCompletePageIssue_11
{
    [Test]
    [Description("Issue #11 - IsCompletePage address check should read an Int32BE, not Int32LE.")]
    public void ShouldReadCompletePages()
    {
        using var fs = new FileStream("Resources/CompletePageIssue-11.tps", FileMode.Open, FileAccess.Read);

        var tpsFile = new TpsFile(fs);

        IEnumerable<TpsBlock>? blocks = null;

        Assert.DoesNotThrow(() =>
        {
            blocks = tpsFile.GetBlocks();
        });

        if (blocks is null)
        {
            Assert.Fail();
            return;
        }

        var singleBlock = blocks.Single();

        var pages = singleBlock.GetPages();

        var singlePage = pages.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(singlePage.AbsoluteAddress, Is.EqualTo(512));
            Assert.That(singlePage.Flags, Is.Zero);
            Assert.That(singlePage.RecordCount, Is.EqualTo(9));
            Assert.That(singlePage.Size, Is.EqualTo(1526));
            Assert.That(singlePage.SizeUncompressed, Is.EqualTo(2119));
            Assert.That(singlePage.SizeUncompressedExpanded, Is.EqualTo(2161));

            var records = singlePage.GetRecords(tpsFile.ErrorHandlingOptions);

            Assert.That(records, Has.Count.EqualTo(9));
        }
    }
}
