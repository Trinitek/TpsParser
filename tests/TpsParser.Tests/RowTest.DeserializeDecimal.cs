using NUnit.Framework;
using System.Collections.Generic;
using TpsParser.Tests.DeserializerModels;
using TpsParser.Tps.Type;

namespace TpsParser.Tests
{
    public partial class RowTest
    {
        [TestFixture]
        public class DeserializeDecimal
        {
            [TestCaseSource(typeof(ShouldDeserializeDecimalAsDecimalData), nameof(ShouldDeserializeDecimalAsDecimalData.Data))]
            public void ShouldDeserializeDecimalAsDecimal(string value, decimal expected)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                var des = row.Deserialize<DecimalModel>();

                Assert.AreEqual(expected, des.Price);
            }

            [TestCaseSource(typeof(ShouldDeserializeDecimalAsDecimalData), nameof(ShouldDeserializeDecimalAsDecimalData.Data))]
            public void ShouldDeserializeDecimalAsNullableDecimal(string value, decimal expected)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                var des = row.Deserialize<NullableDecimalModel>();

                Assert.AreEqual(expected, des.Price);
            }

            private class ShouldDeserializeDecimalAsDecimalData
            {
                public static IEnumerable<TestCaseData> Data
                {
                    get
                    {
                        yield return new TestCaseData("0", 0m);
                        yield return new TestCaseData("-2.3", -2.3m);
                        yield return new TestCaseData("3.4", 3.4m);
                    }
                }
            }

            [TestCase("0")]
            [TestCase("-2.4")]
            [TestCase("3.5")]
            public void ShouldDeserializeDecimalAsString(string value)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                var des = row.Deserialize<DecimalStringModel>();

                Assert.AreEqual(value, des.Price);
            }

            [TestCase("0")]
            [TestCase("-2.4")]
            [TestCase("3.5")]
            public void ShouldDeserializeDecimalAsTpsStringField(string value)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                var des = row.Deserialize<DecimalTpsStringFieldModel>();

                Assert.AreEqual(value, des.Price);
            }

            [TestCase("0", false)]
            [TestCase("-2.4", true)]
            [TestCase("3.5", true)]
            public void ShouldDeserializeDecimalAsBoolean(string value, bool expected)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                var des = row.Deserialize<DecimalBooleanModel>();

                Assert.AreEqual(expected, des.Price);
            }

            [TestCase("0", false)]
            [TestCase("-2.4", true)]
            [TestCase("3.5", true)]
            public void ShouldDeserializeDecimalAsTpsBooleanField(string value, bool expected)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                var des = row.Deserialize<DecimalTpsBooleanFieldModel>();

                Assert.AreEqual(expected, des.Price);
            }
        }
    }
}
