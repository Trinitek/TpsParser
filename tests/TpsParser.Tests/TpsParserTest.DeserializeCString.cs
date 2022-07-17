using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TpsParser.Tests.DeserializerModels;
using TpsParser.Tps;
using TpsParser.Tps.Header;
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

            private static IFieldDefinitionRecord BuildFieldDefinitionRecord(string name, TpsTypeCode typeCode)
            {
                var mock = new Mock<IFieldDefinitionRecord>();

                mock.Setup(m => m.Name).Returns(name);
                mock.Setup(m => m.Type).Returns(typeCode);

                return mock.Object;
            }

            private static ITableDefinitionRecord BuildTableDefinitionRecord(IReadOnlyList<IFieldDefinitionRecord> fieldDefinitions, IReadOnlyList<IMemoDefinitionRecord> memoDefinitions)
            {
                var mock = new Mock<ITableDefinitionRecord>();

                mock.Setup(m => m.Fields).Returns(fieldDefinitions);
                mock.Setup(m => m.Memos).Returns(memoDefinitions);

                return mock.Object;
            }

            private static IMemoRecord BuildMemoRecord(uint owningRecord, string name, TpsCString value)
            {
                var mock = new Mock<IMemoRecord>();

                var memoDefinitionRecordMock = new Mock<IMemoDefinitionRecord>();
                memoDefinitionRecordMock.Setup(m => m.Name).Returns(name);

                var memoHeaderMock = new Mock<IMemoHeader>();

                memoHeaderMock.Setup(m => m.TableNumber).Returns(1);
                memoHeaderMock.Setup(m => m.OwningRecord).Returns(owningRecord);

                mock.Setup(m => m.Header).Returns(memoHeaderMock.Object);
                mock.Setup(m => m.GetValue(It.IsAny<IMemoDefinitionRecord>())).Returns(value);

                return mock.Object;
            }

            private static IDataRecord BuildDataRecord(uint recordNumber, ITableDefinitionRecord tableDefinitionRecord, IEnumerable<ITpsObject> values)
            {
                var mock = new Mock<IDataRecord>();

                mock.Setup(m => m.RecordNumber).Returns(recordNumber);
                mock.Setup(m => m.TableDefinition).Returns(tableDefinitionRecord);
                mock.Setup(m => m.Values).Returns(values.ToList());

                return mock.Object;
            }

            private static TpsFile BuildTpsFile(
                ITableDefinitionRecord tableDefinitionRecord,
                IEnumerable<IDataRecord> dataRecords,
                IEnumerable<IMemoRecord> memoRecords)
            {
                if (dataRecords is null)
                {
                    throw new ArgumentNullException(nameof(dataRecords));
                }

                if (memoRecords is null)
                {
                    throw new ArgumentNullException(nameof(memoRecords));
                }

                var mock = new Mock<TpsFile>(Parser.DefaultEncoding);

                mock.Setup(m => m.GetTableDefinitions(It.IsAny<bool>()))
                    .Returns(new Dictionary<int, ITableDefinitionRecord>()
                    {
                        { 1, tableDefinitionRecord }
                    });

                mock.Setup(m => m.GetDataRecords(It.IsAny<int>(), It.IsAny<ITableDefinitionRecord>(), It.IsAny<bool>()))
                    .Returns(dataRecords);

                mock.Setup(m => m.GetMemoRecords(It.IsAny<int>(), It.IsAny<bool>()))
                    .Returns(memoRecords);

                return mock.Object;
            }

            private static ITableDefinitionRecord BuildTableDefinitionRecord(IEnumerable<(string name, TpsTypeCode typeCode)> fields, IEnumerable<string> memoNames) =>
                BuildTableDefinitionRecord(
                    fields.Select(tuple => BuildFieldDefinitionRecord(tuple.name, tuple.typeCode)).ToList(),
                    memoNames.Select(name => BuildMemoDefinitionRecord(name)).ToList());

            private static TpsFile BuildTpsFile((string name, ITpsObject value)[] fieldValues, (string name, TpsCString value)[] memoValues)
            {
                var tableDefinitionRecord = BuildTableDefinitionRecord(
                    fieldValues.Select(value => (value.name, value.value.TypeCode)).ToList(),
                    memoValues.Select(value => value.name).ToList());

                var dataRecords = new IDataRecord[] { BuildDataRecord(1, tableDefinitionRecord, fieldValues.Select(value => value.value)) };
                var memoRecords = memoValues.Select(value => BuildMemoRecord(1, value.name, value.value));

                return BuildTpsFile(tableDefinitionRecord, dataRecords, memoRecords);
            }

            [Test]
            public void ShouldDeserializeString()
            {
                var file = BuildTpsFile(new (string, ITpsObject)[] { ("Notes", new TpsCString(" Hello world!     ")) }, new (string, TpsCString)[] { });

                using (var parser = new Parser(file))
                {
                    var deserialized = parser.Deserialize<StringModel>().First();

                    Assert.AreEqual(" Hello world!", deserialized.Notes);
                }
            }

            [Test]
            public void ShouldDeserializeAndTrimString()
            {
                string expected = " Hello world!     ";

                var file = BuildTpsFile(new (string, ITpsObject)[] { ("Notes", new TpsCString(expected)) }, new (string, TpsCString)[] { });

                using (var parser = new Parser(file))
                {
                    var deserialized = parser.Deserialize<StringTrimmingEnabledModel>().First();

                    Assert.AreEqual(" Hello world!", deserialized.Notes);
                }
            }

            [Test]
            public void ShouldDeserializeAndNotTrimString()
            {
                string expected = " Hello world!     ";

                var file = BuildTpsFile(new (string, ITpsObject)[] { ("Notes", new TpsCString(expected)) }, new (string, TpsCString)[] { });

                using (var parser = new Parser(file))
                {
                    var deserialized = parser.Deserialize<StringTrimmingDisabledModel>().First();

                    Assert.AreEqual(expected, deserialized.Notes);
                }
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
                var file = BuildTpsFile(new (string, ITpsObject)[] { ("Notes", new TpsCString(value)) }, new (string, TpsCString)[] { });

                using (var parser = new Parser(file))
                {
                    var des = parser.Deserialize<StringBooleanTpsFieldAttributeModel>().First();

                    Assert.AreEqual(expected, des.HasNotes);
                }
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
                var file = BuildTpsFile(new (string, ITpsObject)[] { ("Notes", new TpsCString(value)) }, new (string, TpsCString)[] { });

                using (var parser = new Parser(file))
                {
                    var des = parser.Deserialize<StringBooleanModel>().First();

                    Assert.AreEqual(expected, des.HasNotes);
                }
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
                var file = BuildTpsFile(new (string, ITpsObject)[] { ("Notes", new TpsCString(value)) }, new (string, TpsCString)[] { });

                using (var parser = new Parser(file))
                {
                    var des = parser.Deserialize<StringBooleanTrueModel>().First();

                    Assert.AreEqual(expected, des.HasNotes);
                }
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
                var file = BuildTpsFile(new (string, ITpsObject)[] { ("Notes", new TpsCString(value)) }, new (string, TpsCString)[] { });

                using (var parser = new Parser(file))
                {
                    var des = parser.Deserialize<StringBooleanFalseModel>().First();

                    Assert.AreEqual(expected, des.HasNotes);
                }
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
                var file = BuildTpsFile(new (string, ITpsObject)[] { ("Notes", new TpsCString(value)) }, new (string, TpsCString)[] { });

                using (var parser = new Parser(file))
                {
                    var des = parser.Deserialize<StringBooleanTrueFalseFallbackTrueModel>().First();

                    Assert.AreEqual(expected, des.HasNotes);
                }
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
                var file = BuildTpsFile(new (string, ITpsObject)[] { ("Notes", new TpsCString(value)) }, new (string, TpsCString)[] { });

                using (var parser = new Parser(file))
                {
                    var des = parser.Deserialize<StringBooleanTrueFalseFallbackFalseModel>().First();

                    Assert.AreEqual(expected, des.HasNotes);
                }
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
                var file = BuildTpsFile(new (string, ITpsObject)[] { ("Notes", new TpsCString(value)) }, new (string, TpsCString)[] { });

                using (var parser = new Parser(file))
                {
                    var des = parser.Deserialize<StringBooleanTrueFalseModel>().First();

                    Assert.AreEqual(expected, des.HasNotes);
                }
            }
        }
    }
}
