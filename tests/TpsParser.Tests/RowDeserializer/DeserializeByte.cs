using NUnit.Framework;
using System.Collections.Generic;
using TpsParser.Tests.DeserializerModels;
using TpsParser.Tps.Type;
using static TpsParser.Tests.RowDeserializer.RowDeserializerExtensions;

namespace TpsParser.Tests.RowDeserializer
{
    [TestFixture]
    public class DeserializeByte
    {
        [TestCaseSource(typeof(ShouldDeserializeByteAsByteData), nameof(ShouldDeserializeByteAsByteData.Data))]
        public byte ShouldDeserializeByteAsByte(byte value)
        {
            var row = BuildRow(1, (DeserializerModel.FieldName, new TpsByte(value)));
            var deserialized = row.Deserialize<DeserializerModel<byte>>();

            return deserialized.Value;
        }

        [TestCaseSource(typeof(ShouldDeserializeByteAsByteData), nameof(ShouldDeserializeByteAsByteData.Data))]
        public byte? ShouldDeserializeByteAsNullableByte(byte value)
        {
            var row = BuildRow(1, (DeserializerModel.FieldName, new TpsByte(value)));
            var deserialized = row.Deserialize<DeserializerModel<byte?>>();

            return deserialized.Value;
        }

        private class ShouldDeserializeByteAsByteData
        {
            public static IEnumerable<TestCaseData> Data
            {
                get
                {
                    yield return new TestCaseData((byte)0).Returns((byte)0);
                    yield return new TestCaseData((byte)49).Returns((byte)49);
                    yield return new TestCaseData(byte.MaxValue).Returns(byte.MaxValue);
                }
            }
        }
    }

    [TestFixture]
    public class DeserializeSbyte
    {
        [TestCaseSource(typeof(ShouldDeserializeByteAsSbyteData), nameof(ShouldDeserializeByteAsSbyteData.Data))]
        public sbyte ShouldDeserializeByteAsSbyte(TpsByte value)
        {
            var row = BuildRow(1, (DeserializerModel.FieldName, value));
            var deserialized = row.Deserialize<DeserializerModel<sbyte>>();

            return deserialized.Value;
        }

        [TestCaseSource(typeof(ShouldDeserializeByteAsSbyteData), nameof(ShouldDeserializeByteAsSbyteData.Data))]
        public sbyte? ShouldDeserializeByteAsNullableSbyte(TpsByte value)
        {
            var row = BuildRow(1, (DeserializerModel.FieldName, value));
            var deserialized = row.Deserialize<DeserializerModel<sbyte>>();

            return deserialized.Value;
        }

        private class ShouldDeserializeByteAsSbyteData
        {
            public static IEnumerable<TestCaseData> Data
            {
                get
                {
                    yield return new TestCaseData(new TpsByte(0)).Returns((sbyte)0);
                    yield return new TestCaseData(new TpsByte(49)).Returns((sbyte)49);
                    yield return new TestCaseData(new TpsByte(byte.MaxValue)).Returns(unchecked((sbyte)byte.MaxValue));
                }
            }
        }
    }

    [TestFixture]
    public class DeserializeShort
    {

    }

    [TestFixture]
    public class DeserializeUshort
    {

    }

    [TestFixture]
    public class DeserializeInt
    {

    }

    [TestFixture]
    public class DeserializeUint
    {

    }

    [TestFixture]
    public class DeserializeLongX
    {
        
    }

    [TestFixture]
    public class DeserializeUlong
    {

    }

    [TestFixture]
    public class DeserializeFloat
    {

    }

    [TestFixture]
    public class DeserializeDouble
    {

    }
}
