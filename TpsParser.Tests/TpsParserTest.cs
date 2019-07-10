using NUnit.Framework;
using System;
using System.Linq;
using TpsParser.Tps.Type;

namespace TpsParser.Tests
{
    [TestFixture]
    public class TpsParserTest
    {
        [Test]
        public void ShouldBuildTableWithMemos()
        {
            using (var parser = new TpsParser("Resources/table-with-memos.tps"))
            {
                var table = parser.BuildTable();

                Assert.AreEqual(3, table.Rows.Count());

                var first = table.Rows.OrderBy(r => r.Id).First();

                Assert.AreEqual(3, first.Values.Count());

                // Fixed length string. Dead area is padded with spaces.
                Assert.AreEqual("Joe Smith".PadRight(64, ' '), first.Values["NAME"].Value);
                Assert.AreEqual(new DateTime(2016, 2, 9), first.Values["DATE"].Value);
                Assert.AreEqual("Joe is a great guy to work with.", first.Values["NOTES"].Value);

                var noMemo = table.Rows.First(r => "John NoNotes".PadRight(64, ' ') == ((TpsString)r.Values["NAME"]).Value);

                Assert.AreEqual(2, noMemo.Values.Count());
                Assert.IsFalse(noMemo.Values.TryGetValue("NOTES", out var _));
            }
        }
    }
}
