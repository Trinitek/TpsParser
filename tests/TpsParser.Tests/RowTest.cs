using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TpsParser.Binary;
using TpsParser.Tests.DeserializerModels;
using TpsParser.Tps.Type;

namespace TpsParser.Tests
{
    public class RowTest
    {
        private static Row BuildRow(int rowNumber, params (string columnName, TpsObject value)[] fields) =>
            new Row(rowNumber, new Dictionary<string, TpsObject>(fields.Select(f => new KeyValuePair<string, TpsObject>(f.columnName, f.value))));

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
                var row = BuildRow(1, ("Date", new TpsDate(new RandomAccess(new byte[] { 0, 0, 0, 0 }))));

                var deserialized = row.Deserialize<NullDateModel>();

                Assert.IsNull(deserialized.Date);
            }

            [Test]
            public void ShouldSetDefaultWhenDeserializingNullDateIntoNonNullableDate()
            {
                var row = BuildRow(1, ("Date", new TpsDate(new RandomAccess(new byte[] { 0, 0, 0, 0 }))));

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
        }

        [TestFixture]
        public class DeserializeTime
        {
            [Test]
            public void ShouldDeserializeTime()
            {
                var time = new TimeSpan(12, 13, 42);

                var row = BuildRow(1, ("Time", new TpsTime(time)));

                var deserialized = row.Deserialize<TimeModel>();

                Assert.AreEqual(time, deserialized.Time);
            }

            [Test]
            public void ShouldDeserializeTimeFromLong()
            {
                int centiseconds = 80085;

                var row = BuildRow(1, ("Time", new TpsLong(centiseconds)));

                var deserialized = row.Deserialize<TimeModel>();

                Assert.AreEqual(new TimeSpan(0, 0, 13, 20, 850), deserialized.Time);
            }
        }

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

            [Test]
            public void ShouldDeserializeAndTrimNullString()
            {
                var row = BuildRow(1, ("Notes", new TpsString(null)));

                var deserialized = row.Deserialize<StringTrimmingEnabledModel>();

                Assert.IsNull(deserialized.Notes);
            }
        }
    }
}
