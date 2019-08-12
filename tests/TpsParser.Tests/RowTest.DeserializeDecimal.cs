using NUnit.Framework;
using System.Collections;
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

            [TestCase("0", (int)0)]
            [TestCase("-2.4", (int)-2)]
            [TestCase("3.5", (int)3)]
            public void ShouldDeserializeDecimalAsInt(string value, int expected)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                var des = row.Deserialize<DecimalIntModel>();

                Assert.AreEqual(expected, des.Price);
            }

            [TestCaseSource(typeof(ShouldDeserializeDecimalAsNullableIntData), nameof(ShouldDeserializeDecimalAsNullableIntData.Data))]
            public void ShouldDeserializeDecimalAsNullableInt(string value, int? expected)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                var des = row.Deserialize<DecimalNullableIntModel>();

                Assert.AreEqual(expected, des.Price);
            }

            private class ShouldDeserializeDecimalAsNullableIntData
            {
                public static IEnumerable Data
                {
                    get
                    {
                        yield return new TestCaseData("0", (int?)0);
                        yield return new TestCaseData("-2.4", (int?)-2);
                        yield return new TestCaseData("3.5", (int?)3);
                    }
                }
            }

            [TestCase("0", (uint)0)]
            [TestCase("3.5", (uint)3)]
            public void ShouldDeserializeDecimalAsUInt(string value, uint expected)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                var des = row.Deserialize<DecimalUIntModel>();

                Assert.AreEqual(expected, des.Price);
            }

            [TestCaseSource(typeof(ShouldThrowWhenDeserializingNegativeData), nameof(ShouldThrowWhenDeserializingNegativeData.Data))]
            public void ShouldThrowWhenDeserializingDecimalAsUInt(string value)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                Assert.Throws<TpsParserException>(() => row.Deserialize<DecimalUIntModel>());
            }

            private class ShouldThrowWhenDeserializingNegativeData
            {
                public static IEnumerable Data
                {
                    get
                    {
                        yield return "-2.4";
                        yield return "-0.1";
                        yield return "-127";
                    }
                }
            }

            [TestCaseSource(typeof(ShouldDeserializeDecimalAsNullableUIntData), nameof(ShouldDeserializeDecimalAsNullableUIntData.Data))]
            public void ShouldDeserializeDecimalAsNullableUInt(string value, uint? expected)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                var des = row.Deserialize<DecimalNullableUIntModel>();

                Assert.AreEqual(expected, des.Price);
            }

            private class ShouldDeserializeDecimalAsNullableUIntData
            {
                public static IEnumerable Data
                {
                    get
                    {
                        yield return new TestCaseData("0", (uint?)0);
                        yield return new TestCaseData("3.5", (uint?)3);
                    }
                }
            }

            [TestCaseSource(typeof(ShouldThrowWhenDeserializingNegativeData), nameof(ShouldThrowWhenDeserializingNegativeData.Data))]
            public void ShouldThrowWhenDeserializingDecimalAsNullableUInt(string value)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                Assert.Throws<TpsParserException>(() => row.Deserialize<DecimalNullableUIntModel>());
            }

            [TestCase("0", (short)0)]
            [TestCase("-2.4", (short)-2)]
            [TestCase("3.5", (short)3)]
            public void ShouldDeserializeDecimalAsShort(string value, short expected)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                var des = row.Deserialize<DecimalShortModel>();

                Assert.AreEqual(expected, des.Price);
            }

            [TestCaseSource(typeof(ShouldDeserializeDecimalAsNullableShortData), nameof(ShouldDeserializeDecimalAsNullableShortData.Data))]
            public void ShouldDeserializeDecimalAsNullableShort(string value, short? expected)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                var des = row.Deserialize<DecimalNullableShortModel>();

                Assert.AreEqual(expected, des.Price);
            }

            private class ShouldDeserializeDecimalAsNullableShortData
            {
                public static IEnumerable Data
                {
                    get
                    {
                        yield return new TestCaseData("0", (short?)0);
                        yield return new TestCaseData("-2.4", (short?)-2);
                        yield return new TestCaseData("3.5", (short?)3);
                    }
                }
            }

            [TestCase("0", (ushort)0)]
            [TestCase("3.5", (ushort)3)]
            public void ShouldDeserializeDecimalAsUShort(string value, ushort expected)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                var des = row.Deserialize<DecimalUShortModel>();

                Assert.AreEqual(expected, des.Price);
            }

            [TestCaseSource(typeof(ShouldThrowWhenDeserializingNegativeData), nameof(ShouldThrowWhenDeserializingNegativeData.Data))]
            public void ShouldThrowWhenDeserializingDecimalAsUShort(string value)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                Assert.Throws<TpsParserException>(() => row.Deserialize<DecimalUShortModel>());
            }

            [TestCaseSource(typeof(ShouldDeserializeDecimalAsNullableUShortData), nameof(ShouldDeserializeDecimalAsNullableUShortData.Data))]
            public void ShouldDeserializeDecimalAsNullableUShort(string value, ushort? expected)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                var des = row.Deserialize<DecimalNullableUShortModel>();

                Assert.AreEqual(expected, des.Price);
            }

            private class ShouldDeserializeDecimalAsNullableUShortData
            {
                public static IEnumerable Data
                {
                    get
                    {
                        yield return new TestCaseData("0", (ushort?)0);
                        yield return new TestCaseData("3.5", (ushort?)3);
                    }
                }
            }

            [TestCaseSource(typeof(ShouldThrowWhenDeserializingNegativeData), nameof(ShouldThrowWhenDeserializingNegativeData.Data))]
            public void ShouldThrowWhenDeserializingDecimalAsNullableUShort(string value)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                Assert.Throws<TpsParserException>(() => row.Deserialize<DecimalNullableUShortModel>());
            }

            [TestCase("0", (long)0)]
            [TestCase("-2.4", (long)-2)]
            [TestCase("3.5", (long)3)]
            public void ShouldDeserializeDecimalAsLong(string value, long expected)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                var des = row.Deserialize<DecimalLongModel>();

                Assert.AreEqual(expected, des.Price);
            }

            [TestCaseSource(typeof(ShouldDeserializeDecimalAsNullableLongData), nameof(ShouldDeserializeDecimalAsNullableLongData.Data))]
            public void ShouldDeserializeDecimalAsNullableLong(string value, long? expected)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                var des = row.Deserialize<DecimalNullableLongModel>();

                Assert.AreEqual(expected, des.Price);
            }

            private class ShouldDeserializeDecimalAsNullableLongData
            {
                public static IEnumerable Data
                {
                    get
                    {
                        yield return new TestCaseData("0", (long?)0);
                        yield return new TestCaseData("-2.4", (long?)-2);
                        yield return new TestCaseData("3.5", (long?)3);
                    }
                }
            }

            [TestCase("0", (ulong)0)]
            [TestCase("3.5", (ulong)3)]
            public void ShouldDeserializeDecimalAsULong(string value, ulong expected)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                var des = row.Deserialize<DecimalULongModel>();

                Assert.AreEqual(expected, des.Price);
            }

            [TestCaseSource(typeof(ShouldThrowWhenDeserializingNegativeData), nameof(ShouldThrowWhenDeserializingNegativeData.Data))]
            public void ShouldThrowWhenDeserializingDecimalAsULong(string value)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                Assert.Throws<TpsParserException>(() => row.Deserialize<DecimalULongModel>());
            }

            [TestCaseSource(typeof(ShouldDeserializeDecimalAsNullableULongData), nameof(ShouldDeserializeDecimalAsNullableULongData.Data))]
            public void ShouldDeserializeDecimalAsNullableULong(string value, ulong? expected)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                var des = row.Deserialize<DecimalNullableULongModel>();

                Assert.AreEqual(expected, des.Price);
            }

            private class ShouldDeserializeDecimalAsNullableULongData
            {
                public static IEnumerable Data
                {
                    get
                    {
                        yield return new TestCaseData("0", (ulong?)0);
                        yield return new TestCaseData("3.5", (ulong?)3);
                    }
                }
            }

            [TestCaseSource(typeof(ShouldThrowWhenDeserializingNegativeData), nameof(ShouldThrowWhenDeserializingNegativeData.Data))]
            public void ShouldThrowWhenDeserializingDecimalAsNullableULong(string value)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                Assert.Throws<TpsParserException>(() => row.Deserialize<DecimalNullableULongModel>());
            }

            [TestCase("0", (byte)0)]
            [TestCase("3.5", (byte)3)]
            public void ShouldDeserializeDecimalAsByte(string value, byte expected)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                var des = row.Deserialize<DecimalByteModel>();

                Assert.AreEqual(expected, des.Price);
            }

            [TestCaseSource(typeof(ShouldThrowWhenDeserializingNegativeData), nameof(ShouldThrowWhenDeserializingNegativeData.Data))]
            public void ShouldThrowWhenDeserializingDecimalAsByte(string value)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                Assert.Throws<TpsParserException>(() => row.Deserialize<DecimalByteModel>());
            }

            [TestCaseSource(typeof(ShouldDeserializeDecimalAsNullableByteData), nameof(ShouldDeserializeDecimalAsNullableByteData.Data))]
            public void ShouldDeserializeDecimalAsNullableByte(string value, byte? expected)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                var des = row.Deserialize<DecimalNullableByteModel>();

                Assert.AreEqual(expected, des.Price);
            }

            private class ShouldDeserializeDecimalAsNullableByteData
            {
                public static IEnumerable Data
                {
                    get
                    {
                        yield return new TestCaseData("0", (byte?)0);
                        yield return new TestCaseData("3.5", (byte?)3);
                    }
                }
            }

            [TestCaseSource(typeof(ShouldThrowWhenDeserializingNegativeData), nameof(ShouldThrowWhenDeserializingNegativeData.Data))]
            public void ShouldThrowWhenDeserializingDecimalAsNullableByte(string value)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                Assert.Throws<TpsParserException>(() => row.Deserialize<DecimalNullableByteModel>());
            }

            [TestCase("0", (sbyte)0)]
            [TestCase("-2.4", (sbyte)-2)]
            [TestCase("3.5", (sbyte)3)]
            public void ShouldDeserializeDecimalAsSByte(string value, sbyte expected)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                var des = row.Deserialize<DecimalSByteModel>();

                Assert.AreEqual(expected, des.Price);
            }

            [TestCaseSource(typeof(ShouldDeserializeDecimalAsNullableSByteData), nameof(ShouldDeserializeDecimalAsNullableSByteData.Data))]
            public void ShouldDeserializeDecimalAsNullableSByte(string value, sbyte? expected)
            {
                var row = BuildRow(1, ("Price", new TpsDecimal(value)));
                var des = row.Deserialize<DecimalNullableSByteModel>();

                Assert.AreEqual(expected, des.Price);
            }

            private class ShouldDeserializeDecimalAsNullableSByteData
            {
                public static IEnumerable Data
                {
                    get
                    {
                        yield return new TestCaseData("0", (long?)0);
                        yield return new TestCaseData("-2.4", (long?)-2);
                        yield return new TestCaseData("3.5", (long?)3);
                    }
                }
            }
        }
    }
}
