using NUnit.Framework;
using TpsParser.Tests.DeserializerModels;
using TpsParser.Tps.Type;

namespace TpsParser.Tests
{
    public partial class RowTest
    {
        [TestFixture]
        public class DeserializeString
        {
            [Test]
            public void ShouldDeserializeString()
            {
                string expected = " Hello world!     ";

                var row = BuildRow(1, ("Notes", new TpsString(expected)));

                var deserialized = row.Deserialize<StringModel>();

                Assert.That(deserialized.Notes, Is.EqualTo(expected));
            }

            [Test]
            public void ShouldDeserializeAndTrimString()
            {
                var row = BuildRow(1, ("Notes", new TpsString(" Hello world!     ")));

                var deserialized = row.Deserialize<StringTrimmingEnabledModel>();

                Assert.That(deserialized.Notes, Is.EqualTo(" Hello world!"));
            }

            [Test]
            public void ShouldDeserializeAndNotTrimString()
            {
                string expected = " Hello world!     ";

                var row = BuildRow(1, ("Notes", new TpsString(expected)));

                var deserialized = row.Deserialize<StringTrimmingDisabledModel>();

                Assert.That(deserialized.Notes, Is.EqualTo(expected));
            }

            [TestCase("Y", true)]
            [TestCase("N", true)]
            [TestCase("y", true)]
            [TestCase("n", true)]
            [TestCase("", false)]
            [TestCase(" ", false)]
            [TestCase("?", true)]
            [TestCase(" Y ", true)]
            [TestCase(" N ", true)]
            [TestCase(" y ", true)]
            [TestCase(" n ", true)]
            [TestCase(" ? ", true)]
            public void ShouldDeserializeStringAsBooleanTpsFieldAttribute(string value, bool expected)
            {
                var row = BuildRow(1, ("Notes", new TpsString(value)));

                var des = row.Deserialize<StringBooleanTpsFieldAttributeModel>();

                Assert.That(des.HasNotes, Is.EqualTo(expected));
            }

            [TestCase("Y", true)]
            [TestCase("N", true)]
            [TestCase("y", true)]
            [TestCase("n", true)]
            [TestCase("", false)]
            [TestCase(" ", false)]
            [TestCase("?", true)]
            [TestCase(" Y ", true)]
            [TestCase(" N ", true)]
            [TestCase(" y ", true)]
            [TestCase(" n ", true)]
            [TestCase(" ? ", true)]
            public void ShouldDeserializeStringAsBooleanTpsBooleanFieldAttribute(string value, bool expected)
            {
                var row = BuildRow(1, ("Notes", new TpsString(value)));

                var des = row.Deserialize<StringBooleanModel>();

                Assert.That(des.HasNotes, Is.EqualTo(expected));
            }

            [TestCase("Y", true)]
            [TestCase("N", false)]
            [TestCase("y", true)]
            [TestCase("n", false)]
            [TestCase("", false)]
            [TestCase(" ", false)]
            [TestCase("?", false)]
            [TestCase(" Y ", true)]
            [TestCase(" N ", false)]
            [TestCase(" y ", true)]
            [TestCase(" n ", false)]
            [TestCase(" ? ", false)]
            public void ShouldDeserializeStringAsBooleanTrueCondition(string value, bool expected)
            {
                var row = BuildRow(1, ("Notes", new TpsString(value)));

                var des = row.Deserialize<StringBooleanTrueModel>();

                Assert.That(des.HasNotes, Is.EqualTo(expected));
            }

            [TestCase("Y", true)]
            [TestCase("N", false)]
            [TestCase("y", true)]
            [TestCase("n", false)]
            [TestCase("", false)]
            [TestCase(" ", false)]
            [TestCase("?", true)]
            [TestCase(" Y ", true)]
            [TestCase(" N ", false)]
            [TestCase(" y ", true)]
            [TestCase(" n ", false)]
            [TestCase(" ? ", true)]
            public void ShouldDeserializeStringAsBooleanFalseCondition(string value, bool expected)
            {
                var row = BuildRow(1, ("Notes", new TpsString(value)));

                var des = row.Deserialize<StringBooleanFalseModel>();

                Assert.That(des.HasNotes, Is.EqualTo(expected));
            }

            [TestCase("Y", true)]
            [TestCase("N", false)]
            [TestCase("y", true)]
            [TestCase("n", false)]
            [TestCase("", true)]
            [TestCase(" ", true)]
            [TestCase("?", true)]
            [TestCase(" Y ", true)]
            [TestCase(" N ", false)]
            [TestCase(" y ", true)]
            [TestCase(" n ", false)]
            [TestCase(" ? ", true)]
            public void ShouldDeserializeStringAsBooleanFallbackTrue(string value, bool expected)
            {
                var row = BuildRow(1, ("Notes", new TpsString(value)));

                var des = row.Deserialize<StringBooleanTrueFalseFallbackTrueModel>();

                Assert.That(des.HasNotes, Is.EqualTo(expected));
            }

            [TestCase("Y", true)]
            [TestCase("N", false)]
            [TestCase("y", true)]
            [TestCase("n", false)]
            [TestCase("", false)]
            [TestCase(" ", false)]
            [TestCase("?", false)]
            [TestCase(" Y ", true)]
            [TestCase(" N ", false)]
            [TestCase(" y ", true)]
            [TestCase(" n ", false)]
            [TestCase(" ? ", false)]
            public void ShouldDeserializeStringAsBooleanFallbackFalse(string value, bool expected)
            {
                var row = BuildRow(1, ("Notes", new TpsString(value)));

                var des = row.Deserialize<StringBooleanTrueFalseFallbackFalseModel>();

                Assert.That(des.HasNotes, Is.EqualTo(expected));
            }

            [TestCase("Y", true)]
            [TestCase("N", false)]
            [TestCase("y", true)]
            [TestCase("n", false)]
            [TestCase("", false)]
            [TestCase(" ", false)]
            [TestCase("?", false)]
            [TestCase(" Y ", true)]
            [TestCase(" N ", false)]
            [TestCase(" y ", true)]
            [TestCase(" n ", false)]
            [TestCase(" ? ", false)]
            public void ShouldDeserializeStringAsBooleanFallbackDefault(string value, bool expected)
            {
                var row = BuildRow(1, ("Notes", new TpsString(value)));

                var des = row.Deserialize<StringBooleanTrueFalseModel>();

                Assert.That(des.HasNotes, Is.EqualTo(expected));
            }
        }
    }
}
