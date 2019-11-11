using NUnit.Framework;
using TpsParser.Tests.DeserializerModels;
using TpsParser.Tps.Type;
using static TpsParser.Tests.RowDeserializer.RowDeserializerExtensions;

namespace TpsParser.Tests.RowDeserializer
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

            Assert.AreEqual(expected, deserialized.Notes);
        }

        [Test]
        public void ShouldDeserializeAndTrimString()
        {
            var row = BuildRow(1, ("Notes", new TpsString(" Hello world!     ")));

            var deserialized = row.Deserialize<StringTrimmingEnabledModel>();

            Assert.AreEqual(" Hello world!", deserialized.Notes);
        }

        [Test]
        public void ShouldDeserializeAndNotTrimString()
        {
            string expected = " Hello world!     ";

            var row = BuildRow(1, ("Notes", new TpsString(expected)));

            var deserialized = row.Deserialize<StringTrimmingDisabledModel>();

            Assert.AreEqual(expected, deserialized.Notes);
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

            Assert.AreEqual(expected, des.HasNotes);
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

            Assert.AreEqual(expected, des.HasNotes);
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

            Assert.AreEqual(expected, des.HasNotes);
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

            Assert.AreEqual(expected, des.HasNotes);
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

            Assert.AreEqual(expected, des.HasNotes);
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

            Assert.AreEqual(expected, des.HasNotes);
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

            Assert.AreEqual(expected, des.HasNotes);
        }
    }
}
