using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using TpsParser.Tests.DeserializerModels;
using TpsParser.Tps;
using TpsParser.Tps.Record;
using TpsParser.Tps.Type;

namespace TpsParser.Tests
{
    public partial class TpsParserTest
    {
        [TestFixture]
        public class DeserializeCString
        {
            private static IMemoDefinitionRecord BuildMemoDefinitionRecord(string name)
            {
                var mock = new Mock<IMemoDefinitionRecord>();

                mock.Setup(m => m.Name).Returns(name);
                mock.Setup(m => m.IsMemo).Returns(true);
                mock.Setup(m => m.IsBlob).Returns(false);

                return mock.Object;
            }

            private static IFieldDefinitionRecord BuildFieldDefinitionRecord(string name)
            {
                var mock = new Mock<IFieldDefinitionRecord>();

                mock.Setup(m => m.Name).Returns(name);

                return mock.Object;
            }

            private static ITableDefinitionRecord BuildTableDefinitionRecord(IReadOnlyList<IFieldDefinitionRecord> fieldDefinitions, IReadOnlyList<IMemoDefinitionRecord> memoDefinitions)
            {
                var mock = new Mock<ITableDefinitionRecord>();

                mock.Setup(m => m.Fields).Returns(fieldDefinitions);
                mock.Setup(m => m.Memos).Returns(memoDefinitions);

                return mock.Object;
            }

            private static TpsFile BuildTpsFile(Func<IEnumerable<DataRecord>> dataRecordFunc, Func<IEnumerable<MemoRecord>> memoRecordFunc)
            {
                if (dataRecordFunc is null)
                {
                    throw new ArgumentNullException(nameof(dataRecordFunc));
                }

                if (memoRecordFunc is null)
                {
                    throw new ArgumentNullException(nameof(memoRecordFunc));
                }

                var mock = new Mock<TpsFile>();

                mock.Setup(m => m.GetTableDefinitions(It.IsAny<bool>()))
                    .Returns(new Dictionary<int, ITableDefinitionRecord>()
                    {
                        { 1, BuildTableDefinitionRecord() }
                    });

                mock.Setup(m => m.GetDataRecords(It.IsAny<int>(), It.IsAny<ITableDefinitionRecord>(), It.IsAny<bool>()))
                    .Returns(dataRecordFunc);

                mock.Setup(m => m.GetMemoRecords(It.IsAny<int>(), It.IsAny<bool>()))
                    .Returns(memoRecordFunc);

                return mock.Object;
            }

            [Test]
            public void ShouldDeserializeString()
            {
                string expected = " Hello world!     ";

                using (var parser = new TpsParser())

                var row = BuildRow(1, ("Notes", new TpsCString(expected)));

                var deserialized = row.Deserialize<StringModel>();

                Assert.AreEqual(expected, deserialized.Notes);
            }

            [Test]
            public void ShouldDeserializeAndTrimString()
            {
                var row = BuildRow(1, ("Notes", new TpsCString(" Hello world!     ")));

                var deserialized = row.Deserialize<StringTrimmingEnabledModel>();

                Assert.AreEqual(" Hello world!", deserialized.Notes);
            }

            [Test]
            public void ShouldDeserializeAndNotTrimString()
            {
                string expected = " Hello world!     ";

                var row = BuildRow(1, ("Notes", new TpsCString(expected)));

                var deserialized = row.Deserialize<StringTrimmingDisabledModel>();

                Assert.AreEqual(expected, deserialized.Notes);
            }

            [TestCase("Y", true)]
            [TestCase("N", true)]
            [TestCase("y", true)]
            [TestCase("n", true)]
            [TestCase("", false)]
            [TestCase(" ", true)]
            [TestCase("?", true)]
            public void ShouldDeserializeStringAsBooleanTpsFieldAttribute(string value, bool expected)
            {
                var row = BuildRow(1, ("Notes", new TpsCString(value)));

                var des = row.Deserialize<StringBooleanTpsFieldAttributeModel>();

                Assert.AreEqual(expected, des.HasNotes);
            }

            [TestCase("Y", true)]
            [TestCase("N", true)]
            [TestCase("y", true)]
            [TestCase("n", true)]
            [TestCase("", false)]
            [TestCase(" ", true)]
            [TestCase("?", true)]
            public void ShouldDeserializeStringAsBooleanTpsBooleanFieldAttribute(string value, bool expected)
            {
                var row = BuildRow(1, ("Notes", new TpsCString(value)));

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
            public void ShouldDeserializeStringAsBooleanTrueCondition(string value, bool expected)
            {
                var row = BuildRow(1, ("Notes", new TpsCString(value)));

                var des = row.Deserialize<StringBooleanTrueModel>();

                Assert.AreEqual(expected, des.HasNotes);
            }

            [TestCase("Y", true)]
            [TestCase("N", false)]
            [TestCase("y", true)]
            [TestCase("n", false)]
            [TestCase("", false)]
            [TestCase(" ", true)]
            [TestCase("?", true)]
            public void ShouldDeserializeStringAsBooleanFalseCondition(string value, bool expected)
            {
                var row = BuildRow(1, ("Notes", new TpsCString(value)));

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
            public void ShouldDeserializeStringAsBooleanFallbackTrue(string value, bool expected)
            {
                var row = BuildRow(1, ("Notes", new TpsCString(value)));

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
            public void ShouldDeserializeStringAsBooleanFallbackFalse(string value, bool expected)
            {
                var row = BuildRow(1, ("Notes", new TpsCString(value)));

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
            public void ShouldDeserializeStringAsBooleanFallbackDefault(string value, bool expected)
            {
                var row = BuildRow(1, ("Notes", new TpsCString(value)));

                var des = row.Deserialize<StringBooleanTrueFalseModel>();

                Assert.AreEqual(expected, des.HasNotes);
            }
        }
    }
}
