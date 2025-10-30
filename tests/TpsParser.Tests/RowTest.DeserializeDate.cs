using NUnit.Framework;
using System;
using TpsParser.Binary;
using TpsParser.Tests.DeserializerModels;
using TpsParser.Tps.Type;

namespace TpsParser.Tests
{
    public partial class RowTest
    {
        public class DeserializeDate
        {
            [Test]
            public void ShouldDeserializeDate()
            {
                var date = new DateTime(2019, 7, 17);

                var row = BuildRow(1, ("Date", new TpsDate(date)));

                var deserialized = row.Deserialize<DateModel>();

                Assert.That(deserialized.Date, Is.EqualTo(date));
            }

            [Test]
            public void ShouldDeserializeDateFromLong()
            {
                int clarionStandardDate = 80085;

                var row = BuildRow(1, ("Date", new TpsLong(clarionStandardDate)));

                var deserialized = row.Deserialize<DateModel>();

                Assert.That(deserialized.Date, Is.EqualTo(new DateTime(2020, 4, 3)));
            }

            [Test]
            public void ShouldDeserializeNullDate()
            {
                var row = BuildRow(1, ("Date", new TpsDate(new TpsRandomAccess(new byte[] { 0, 0, 0, 0 }))));

                var deserialized = row.Deserialize<NullDateModel>();

                Assert.That(deserialized.Date, Is.Null);
            }

            [Test]
            public void ShouldSetDefaultWhenDeserializingNullDateIntoNonNullableDate()
            {
                var row = BuildRow(1, ("Date", new TpsDate(new TpsRandomAccess(new byte[] { 0, 0, 0, 0 }))));

                var deserialized = row.Deserialize<DateModel>();

                Assert.That(deserialized.Date, Is.Default);
            }

            [Test]
            public void ShouldDeserializeDateString()
            {
                var expected = new DateTime(2019, 7, 17);

                var row = BuildRow(1, ("Date", new TpsDate(expected)));

                var deserialized = row.Deserialize<DateStringModel>();

                Assert.That(deserialized.Date, Is.EqualTo(expected.ToString()));
            }

            [Test]
            public void ShouldDeserializeDateStringFormatted()
            {
                var expected = new DateTime(2019, 7, 17);

                var row = BuildRow(1, ("Date", new TpsDate(expected)));

                var deserialized = row.Deserialize<DateStringFormattedModel>();

                Assert.That(deserialized.Date, Is.EqualTo(expected.ToString("MM - dd - yyyy")));
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

                Assert.That(deserialized.Date, Is.EqualTo("nothing"));
            }

            [Test]
            public void ShouldDeserializeDateFromDecimal()
            {
                int clarionStandardDate = 80085;

                var row = BuildRow(1, ("Date", new TpsDecimal(clarionStandardDate.ToString())));

                var deserialized = row.Deserialize<DateModel>();

                Assert.That(deserialized.Date, Is.EqualTo(new DateTime(2020, 4, 3)));
            }

            [Test]
            public void ShouldDeserializeNullableDateFromDecimal()
            {
                int clarionStandardDate = 80085;

                var row = BuildRow(1, ("Date", new TpsDecimal(clarionStandardDate.ToString())));

                var deserialized = row.Deserialize<NullDateModel>();

                Assert.That(deserialized.Date, Is.EqualTo(new DateTime(2020, 4, 3)));
            }
        }
    }
}
