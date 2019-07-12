using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TpsParser.Tps.Type;

namespace TpsParser.Tests
{
    [TestFixture]
    public partial class TpsParserTest
    {
        [Test]
        public void ShouldBuildTableWithMemos()
        {
            using (var parser = new TpsParser("Resources/table-with-memos.tps"))
            {
                var table = parser.BuildTable();

                Assert.AreEqual(3, table.Rows.Count());

                var rows = table.Rows.OrderBy(r => r.Id).ToList();

                Assert.AreEqual(3, rows.Count());

                // Fixed length strings. Dead area is padded with spaces, except for memos.

                Assert.AreEqual(3, rows[0].Values.Count());
                Assert.AreEqual("Joe Smith".PadRight(64, ' '), rows[0].Values["NAME"].Value);
                Assert.AreEqual(new DateTime(2016, 2, 9), rows[0].Values["DATE"].Value);
                Assert.AreEqual("Joe is a great guy to work with.", rows[0].Values["NOTES"].Value);

                Assert.AreEqual(3, rows[1].Values.Count());
                Assert.AreEqual("Jane Smith".PadRight(64, ' '), rows[1].Values["NAME"].Value);
                Assert.AreEqual(new DateTime(2019, 8, 22), rows[1].Values["DATE"].Value);
                Assert.AreEqual("Jane knows how to make a great pot of coffee.", rows[1].Values["NOTES"].Value);

                Assert.AreEqual(2, rows[2].Values.Count());
                Assert.AreEqual("John NoNotes".PadRight(64, ' '), rows[2].Values["NAME"].Value);
                Assert.AreEqual(new DateTime(2016, 2, 9), rows[2].Values["DATE"].Value);
                Assert.IsFalse(rows[2].Values.TryGetValue("NOTES", out var _));
            }
        }

        private IEnumerable<T> InvokeDeserialize<T>(TpsParser parser, Type targetObjectType, bool ignoreErrors)
        {
            // parser.Deserialize<T>()
            return (IEnumerable<T>)parser.GetType()
                .GetMethod(nameof(TpsParser.Deserialize))
                .MakeGenericMethod(targetObjectType)
                .Invoke(parser, new object[] { ignoreErrors });
        }

        [TestCase(typeof(DeserializeMemosInternalFields))]
        [TestCase(typeof(DeserializeMemosInternalSetters))]
        [TestCase(typeof(DeserializeMemosPrivateFields))]
        [TestCase(typeof(DeserializeMemosPrivateSetters))]
        [TestCase(typeof(DeserializeMemosProtectedFields))]
        [TestCase(typeof(DeserializeMemosProtectedSetters))]
        [TestCase(typeof(DeserializeMemosPublicFields))]
        [TestCase(typeof(DeserializeMemosPublicSetters))]
        public void ShouldDeserializeMemos(Type targetObjectType)
        {
            using (var parser = new TpsParser("Resources/table-with-memos.tps"))
            {
                var rows = InvokeDeserialize<IDeserializeMemos>(parser, targetObjectType, ignoreErrors: false);

                var first = rows.First();

                Assert.AreEqual(3, rows.Count());
                Assert.AreEqual("Joe Smith".PadRight(64, ' '), first.Name);
                Assert.AreEqual(new DateTime(2016, 2, 9), first.Date);
                Assert.AreEqual("Joe is a great guy to work with.", first.Notes);
            }
        }

        [Test]
        public void ShouldThrowMissingFieldWhenDeserializingMemos()
        {
            using (var parser = new TpsParser("Resources/table-with-memos.tps"))
            {
                Assert.Throws<TpsParserException>(() => parser.Deserialize<DeserializeMemosNotesRequired>().ToList());
            }
        }
    }
}
