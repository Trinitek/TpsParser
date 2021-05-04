using NUnit.Framework;
using System;
using TpsParser.Tests.DeserializerModels;
using TpsParser.Tps.Type;
using static TpsParser.Tests.RowDeserializer.RowDeserializerExtensions;

namespace TpsParser.Tests.RowDeserializer
{
    [TestFixture]
    public class DeserializeDate
    {
        [Test]
        public void ShouldDeserializeDate()
        {
            var date = new DateTime(2019, 7, 17);

            var row = BuildRow(1, ("Date", new TpsDate(date)));

            var deserialized = row.Deserialize<DateModel>();

            Assert.AreEqual(date, deserialized.Date);
        }

        [Test]
        public void ShouldDeserializeDateFromLong()
        {
            int clarionStandardDate = 80085;

            var row = BuildRow(1, ("Date", new TpsLong(clarionStandardDate)));

            var deserialized = row.Deserialize<DateModel>();

            Assert.AreEqual(new DateTime(2020, 4, 3), deserialized.Date);
        }

        [Test]
        public void ShouldDeserializeNullDate()
        {
            var row = BuildRow(1, ("Date", new TpsDate(new TpsReader(new byte[] { 0, 0, 0, 0 }))));

            var deserialized = row.Deserialize<NullDateModel>();

            Assert.IsNull(deserialized.Date);
        }

        [Test]
        public void ShouldSetDefaultWhenDeserializingNullDateIntoNonNullableDate()
        {
            var row = BuildRow(1, ("Date", new TpsDate(new TpsReader(new byte[] { 0, 0, 0, 0 }))));

            var deserialized = row.Deserialize<DateModel>();

            Assert.AreEqual(default(DateTime), deserialized.Date);
        }

        [Test]
        public void ShouldDeserializeDateString()
        {
            var expected = new DateTime(2019, 7, 17);

            var row = BuildRow(1, ("Date", new TpsDate(expected)));

            var deserialized = row.Deserialize<DateStringModel>();

            Assert.AreEqual(expected.ToString(), deserialized.Date);
        }

        [Test]
        public void ShouldDeserializeDateStringFormatted()
        {
            var expected = new DateTime(2019, 7, 17);

            var row = BuildRow(1, ("Date", new TpsDate(expected)));

            var deserialized = row.Deserialize<DateStringFormattedModel>();

            Assert.AreEqual(expected.ToString("MM - dd - yyyy"), deserialized.Date);
        }

        [Test]
        public void ShouldThrowDeserializingDateStringToNonStringMember()
        {
            var row = BuildRow(1, ("Date", new TpsDate(new DateTime(2019, 7, 17))));

            Assert.Throws<TpsParserException>(() => row.Deserialize<DateStringNonStringMemberModel>());
        }

        [Test]
        public void ShouldUseFallbackDeserializingNullDate()
        {
            var row = BuildRow(1, ("Date", new TpsDate((DateTime?)null)));

            var deserialized = row.Deserialize<DateStringFallbackModel>();

            Assert.AreEqual("nothing", deserialized.Date);
        }

        [Test]
        public void ShouldDeserializeDateFromDecimal()
        {
            int clarionStandardDate = 80085;

            var row = BuildRow(1, ("Date", new TpsDecimal(clarionStandardDate.ToString())));

            var deserialized = row.Deserialize<DateModel>();

            Assert.AreEqual(new DateTime(2020, 4, 3), deserialized.Date);
        }

        [Test]
        public void ShouldDeserializeNullableDateFromDecimal()
        {
            int clarionStandardDate = 80085;

            var row = BuildRow(1, ("Date", new TpsDecimal(clarionStandardDate.ToString())));

            var deserialized = row.Deserialize<NullDateModel>();

            Assert.AreEqual(new DateTime(2020, 4, 3), deserialized.Date);
        }
    }
}
