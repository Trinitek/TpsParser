using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TpsParser.Tests.DeserializerModels;

namespace TpsParser.Tests.ParserIntegrations
{
    [TestFixture]
    public class TableWithMemos
    {
        [Test]
        public void ShouldBuildTableWithMemos()
        {
            using (var parser = new TpsParser("Resources/table-with-memos.tps"))
            {
                var table = parser.BuildTable();
                var rows = table.Rows.OrderBy(r => r.Id).ToList();

                Assert.AreEqual(4, rows.Count());

                // Fixed length strings. Dead area is padded with spaces, except for memos.

                Assert.AreEqual(4, rows[0].Values.Count());
                Assert.AreEqual("Joe Smith".PadRight(64, ' '), rows[0].Values["Name"].Value);
                Assert.AreEqual(new DateTime(2016, 2, 9), rows[0].Values["Date"].Value);
                Assert.AreEqual("Joe is a great guy to work with.", rows[0].Values["Notes"].Value);
                Assert.AreEqual("He also likes sushi.", rows[0].Values["AdditionalNotes"].Value);

                Assert.AreEqual(4, rows[1].Values.Count());
                Assert.AreEqual("Jane Jones".PadRight(64, ' '), rows[1].Values["Name"].Value);
                Assert.AreEqual(new DateTime(2019, 8, 22), rows[1].Values["Date"].Value);
                Assert.AreEqual("Jane knows how to make a great pot of coffee.", rows[1].Values["Notes"].Value);
                Assert.AreEqual("She doesn't like sushi as much as Joe.", rows[1].Values["AdditionalNotes"].Value);

                Assert.AreEqual(2, rows[2].Values.Count());
                Assert.AreEqual("John NoNotes".PadRight(64, ' '), rows[2].Values["Name"].Value);
                Assert.AreEqual(new DateTime(2019, 10, 7), rows[2].Values["Date"].Value);
                Assert.IsFalse(rows[2].Values.TryGetValue("Notes", out var _));
                Assert.IsFalse(rows[2].Values.TryGetValue("AdditionalNotes", out var _));

                Assert.AreEqual(3, rows[3].Values.Count());
                Assert.AreEqual("Jimmy OneNote".PadRight(64, ' '), rows[3].Values["Name"].Value);
                Assert.AreEqual(new DateTime(2013, 3, 14), rows[3].Values["Date"].Value);
                Assert.IsFalse(rows[3].Values.TryGetValue("Notes", out var _));
                Assert.AreEqual("Has a strange last name.", rows[3].Values["AdditionalNotes"].Value);
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

        [TestCase(typeof(MemosInternalFieldsModel))]
        [TestCase(typeof(MemosInternalSettersModel))]
        [TestCase(typeof(MemosPrivateFieldsModel))]
        [TestCase(typeof(MemosPrivateSettersModel))]
        [TestCase(typeof(MemosProtectedFieldsModel))]
        [TestCase(typeof(MemosProtectedSettersModel))]
        [TestCase(typeof(MemosPublicFieldsModel))]
        [TestCase(typeof(MemosPublicSettersModel))]
        public void ShouldDeserializeMemos(Type targetObjectType)
        {
            using (var parser = new TpsParser("Resources/table-with-memos.tps"))
            {
                var rows = InvokeDeserialize<IMemoModel>(parser, targetObjectType, ignoreErrors: false)
                    .ToList();

                Assert.AreEqual(4, rows.Count());

                Assert.AreEqual("Joe Smith".PadRight(64, ' '), rows[0].Name);
                Assert.AreEqual(new DateTime(2016, 2, 9), rows[0].Date);
                Assert.AreEqual("Joe is a great guy to work with.", rows[0].Notes);
                Assert.AreEqual("He also likes sushi.", rows[0].AdditionalNotes);

                Assert.AreEqual("Jane Jones".PadRight(64, ' '), rows[1].Name);
                Assert.AreEqual(new DateTime(2019, 8, 22), rows[1].Date);
                Assert.AreEqual("Jane knows how to make a great pot of coffee.", rows[1].Notes);
                Assert.AreEqual("She doesn't like sushi as much as Joe.", rows[1].AdditionalNotes);

                Assert.AreEqual("John NoNotes".PadRight(64, ' '), rows[2].Name);
                Assert.AreEqual(new DateTime(2019, 10, 7), rows[2].Date);
                Assert.IsNull(rows[2].Notes);
                Assert.IsNull(rows[2].AdditionalNotes);

                Assert.AreEqual("Jimmy OneNote".PadRight(64, ' '), rows[3].Name);
                Assert.AreEqual(new DateTime(2013, 3, 14), rows[3].Date);
                Assert.IsNull(rows[3].Notes);
                Assert.AreEqual("Has a strange last name.", rows[3].AdditionalNotes);
            }
        }

        [Test]
        public void ShouldThrowMissingFieldWhenDeserializingMemos()
        {
            using (var parser = new TpsParser("Resources/table-with-memos.tps"))
            {
                Assert.Throws<TpsParserException>(() => parser.Deserialize<MemosNotesRequiredModel>().ToList());
            }
        }

        [Test]
        public void ShouldDeserializeRecordNumberField()
        {
            using (var parser = new TpsParser("Resources/table-with-memos.tps"))
            {
                var rows = parser.Deserialize<MemosRecordNumberFieldModel>().ToList();

                Assert.AreEqual(2, rows[0]._id);
                Assert.AreEqual(3, rows[1]._id);
                Assert.AreEqual(4, rows[2]._id);
                Assert.AreEqual(5, rows[3]._id);
            }
        }

        [Test]
        public void ShouldDeserializeRecordNumberProperty()
        {
            using (var parser = new TpsParser("Resources/table-with-memos.tps"))
            {
                var rows = parser.Deserialize<MemosRecordNumberPropertyModel>().ToList();

                Assert.AreEqual(2, rows[0].Id);
                Assert.AreEqual(3, rows[1].Id);
                Assert.AreEqual(4, rows[2].Id);
                Assert.AreEqual(5, rows[3].Id);
            }
        }

        [Test]
        public void ShouldThrowRecordNumberAndFieldAttrOnSameMember()
        {
            using (var parser = new TpsParser("Resources/table-with-memos.tps"))
            {
                Assert.Throws<TpsParserException>(() => parser.Deserialize<MemosRecordNumberAndFieldAttrOnSameMemberModel>().ToList());
            }
        }

        [Test]
        public void ShouldCancelAsync()
        {
            using (var parser = new TpsParser("Resources/table-with-memos.tps"))
            using (var cts = new CancellationTokenSource())
            {
                cts.Cancel();

                Assert.ThrowsAsync<OperationCanceledException>(async () =>
                {
                    var enumerable = await parser.DeserializeAsync<MemosNotesRequiredModel>(ct: cts.Token);
                    enumerable.ToList();
                });
            }
        }
    }
}
