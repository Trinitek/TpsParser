using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TpsParser.Tests.DeserializerModels;

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
                var rows = table.Rows.OrderBy(r => r.Id).ToList();

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(rows.Count(), Is.EqualTo(4));

                    // Fixed length strings. Dead area is padded with spaces, except for memos.

                    Assert.That(rows[0].Values.Count(), Is.EqualTo(4));
                    Assert.That(rows[0].Values["Name"].Value, Is.EqualTo("Joe Smith".PadRight(64, ' ')));
                    Assert.That(rows[0].Values["Date"].Value, Is.EqualTo(new DateTime(2016, 2, 9)));
                    Assert.That(rows[0].Values["Notes"].Value, Is.EqualTo("Joe is a great guy to work with."));
                    Assert.That(rows[0].Values["AdditionalNotes"].Value, Is.EqualTo("He also likes sushi."));

                    Assert.That(rows[1].Values.Count(), Is.EqualTo(4));
                    Assert.That(rows[1].Values["Name"].Value, Is.EqualTo("Jane Jones".PadRight(64, ' ')));
                    Assert.That(rows[1].Values["Date"].Value, Is.EqualTo(new DateTime(2019, 8, 22)));
                    Assert.That(rows[1].Values["Notes"].Value, Is.EqualTo("Jane knows how to make a great pot of coffee."));
                    Assert.That(rows[1].Values["AdditionalNotes"].Value, Is.EqualTo("She doesn't like sushi as much as Joe."));

                    Assert.That(rows[2].Values.Count(), Is.EqualTo(2));
                    Assert.That(rows[2].Values["Name"].Value, Is.EqualTo("John NoNotes".PadRight(64, ' ')));
                    Assert.That(rows[2].Values["Date"].Value, Is.EqualTo(new DateTime(2019, 10, 7)));
                    Assert.That(rows[2].Values.TryGetValue("Notes", out var _), Is.False);
                    Assert.That(rows[2].Values.TryGetValue("AdditionalNotes", out var _), Is.False);

                    Assert.That(rows[3].Values.Count(), Is.EqualTo(3));
                    Assert.That(rows[3].Values["Name"].Value, Is.EqualTo("Jimmy OneNote".PadRight(64, ' ')));
                    Assert.That(rows[3].Values["Date"].Value, Is.EqualTo(new DateTime(2013, 3, 14)));
                    Assert.That(rows[3].Values.TryGetValue("Notes", out var _), Is.False);
                    Assert.That(rows[3].Values["AdditionalNotes"].Value, Is.EqualTo("Has a strange last name."));
                }
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

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(rows.Count(), Is.EqualTo(4));

                    Assert.That(rows[0].Name, Is.EqualTo("Joe Smith".PadRight(64, ' ')));
                    Assert.That(rows[0].Date, Is.EqualTo(new DateTime(2016, 2, 9)));
                    Assert.That(rows[0].Notes, Is.EqualTo("Joe is a great guy to work with."));
                    Assert.That(rows[0].AdditionalNotes, Is.EqualTo("He also likes sushi."));

                    Assert.That(rows[1].Name, Is.EqualTo("Jane Jones".PadRight(64, ' ')));
                    Assert.That(rows[1].Date, Is.EqualTo(new DateTime(2019, 8, 22)));
                    Assert.That(rows[1].Notes, Is.EqualTo("Jane knows how to make a great pot of coffee."));
                    Assert.That(rows[1].AdditionalNotes, Is.EqualTo("She doesn't like sushi as much as Joe."));

                    Assert.That(rows[2].Name, Is.EqualTo("John NoNotes".PadRight(64, ' ')));
                    Assert.That(rows[2].Date, Is.EqualTo(new DateTime(2019, 10, 7)));
                    Assert.That(rows[2].Notes, Is.Null);
                    Assert.That(rows[2].AdditionalNotes, Is.Null);

                    Assert.That(rows[3].Name, Is.EqualTo("Jimmy OneNote".PadRight(64, ' ')));
                    Assert.That(rows[3].Date, Is.EqualTo(new DateTime(2013, 3, 14)));
                    Assert.That(rows[3].Notes, Is.Null);
                    Assert.That(rows[3].AdditionalNotes, Is.EqualTo("Has a strange last name."));
                }
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

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(rows[0]._id, Is.EqualTo(2));
                    Assert.That(rows[1]._id, Is.EqualTo(3));
                    Assert.That(rows[2]._id, Is.EqualTo(4));
                    Assert.That(rows[3]._id, Is.EqualTo(5));
                }
            }
        }

        [Test]
        public void ShouldDeserializeRecordNumberProperty()
        {
            using (var parser = new TpsParser("Resources/table-with-memos.tps"))
            {
                var rows = parser.Deserialize<MemosRecordNumberPropertyModel>().ToList();

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(rows[0].Id, Is.EqualTo(2));
                    Assert.That(rows[1].Id, Is.EqualTo(3));
                    Assert.That(rows[2].Id, Is.EqualTo(4));
                    Assert.That(rows[3].Id, Is.EqualTo(5));
                }
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
