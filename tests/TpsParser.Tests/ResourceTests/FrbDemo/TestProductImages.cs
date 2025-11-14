using NUnit.Framework;
using System.IO;
using System.Linq;

namespace TpsParser.Tests.ResourceTests.FrbDemo;

internal sealed class TestProductImages
{
    [TestCase(11, "ANTHUR.BMP")]
    [TestCase(12, "BOTTLEBR.BMP")]
    [TestCase(13, "CHINESEL.BMP")]
    [TestCase(14, "CARNATIN.BMP")]
    [TestCase(15, "AMARYLIS.BMP")]
    [TestCase(16, "ALLIUM.BMP")]
    [TestCase(17, "ASTER.BMP")]
    [TestCase(18, "ANTHUR.BMP")]
    [TestCase(2, "BIRD_PAR-rec02.BMP")]
    [TestCase(3, "NO_PICT.BMP")]
    [TestCase(4, "AZAELA.BMP")]
    [TestCase(5, "AZTEC.BMP")]
    [TestCase(6, "BAMBOO-rec06.BMP")]
    [TestCase(7, "BIRD_PAR-rec07.BMP")]
    [TestCase(8, "BLACKEYE.BMP")]
    [TestCase(9, "BLUEBON.BMP")]
    [TestCase(10, "DAISY.BMP")]
    [TestCase(19, "BAMBOO-rec19.BMP")]
    [TestCase(20, "DAFFODIL.BMP")]
    [TestCase(21, "CACTUS.BMP")]
    [TestCase(22, "BAYLEAF.BMP")]
    [TestCase(23, "BEGONIA.BMP")]
    [TestCase(25, "CATTAILS.BMP")]
    [TestCase(26, "CHERRYBL.BMP")]
    [TestCase(27, "CHRYSANT.BMP")]
    [TestCase(28, "CROCUS.BMP")]
    [TestCase(29, "CLOVER.BMP")]
    [TestCase(30, "CRCACTUS.BMP")]
    [TestCase(31, "FERN.BMP")]
    [TestCase(32, "DWARFPET.BMP")]
    public void ProductShouldHaveImageData(int recordNumber, string expectedImageFileName)
    {
        const string tpsFilename = "Resources/frb_demo/Products.tps";
        const string imageBasePath = "Resources/frb_demo/Products_bitmaps";

        string imagePath = Path.Combine(imageBasePath, expectedImageFileName);
        
        byte[] expectedImageData = File.ReadAllBytes(imagePath);

        using var fs = new FileStream(tpsFilename, FileMode.Open, FileAccess.Read);

        var tpsFile = new TpsFile(fs, errorHandlingOptions: ErrorHandlingOptions.Strict);

        var memos = tpsFile.GetTpsMemos(
            table: 1,
            owningRecord: recordNumber,
            memoIndex: 1);

        var blob = (TpsBlob)memos.Single();

        Assert.That(blob.ToArray(), Is.EqualTo(expectedImageData));
    }
}
