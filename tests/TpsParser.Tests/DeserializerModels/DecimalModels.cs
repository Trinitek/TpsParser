namespace TpsParser.Tests.DeserializerModels
{
    public class DecimalModel
    {
        [TpsField("Price")]
        public decimal Price { get; set; }
    }

    public class NullableDecimalModel
    {
        [TpsField("Price")]
        public decimal? Price { get; set; }
    }

    public class DecimalStringModel
    {
        [TpsField("Price")]
        public string Price { get; set; }
    }

    public class DecimalTpsStringFieldModel
    {
        [TpsField("Price")]
        public string Price { get; set; }
    }

    public class DecimalBooleanModel
    {
        [TpsField("Price")]
        public bool Price { get; set; }
    }

    public class DecimalTpsBooleanFieldModel
    {
        [TpsField("Price")]
        public bool Price { get; set; }
    }

    public class DecimalIntModel
    {
        [TpsField("Price")]
        public int Price { get; set; }
    }

    public class DecimalNullableIntModel
    {
        [TpsField("Price")]
        public int? Price { get; set; }
    }

    public class DecimalUIntModel
    {
        [TpsField("Price")]
        public uint Price { get; set; }
    }

    public class DecimalNullableUIntModel
    {
        [TpsField("Price")]
        public uint? Price { get; set; }
    }

    public class DecimalShortModel
    {
        [TpsField("Price")]
        public short Price { get; set; }
    }

    public class DecimalNullableShortModel
    {
        [TpsField("Price")]
        public short? Price { get; set; }
    }

    public class DecimalUShortModel
    {
        [TpsField("Price")]
        public ushort Price { get; set; }
    }

    public class DecimalNullableUShortModel
    {
        [TpsField("Price")]
        public ushort? Price { get; set; }
    }

    public class DecimalLongModel
    {
        [TpsField("Price")]
        public long Price { get; set; }
    }

    public class DecimalNullableLongModel
    {
        [TpsField("Price")]
        public long? Price { get; set; }
    }

    public class DecimalULongModel
    {
        [TpsField("Price")]
        public ulong Price { get; set; }
    }

    public class DecimalNullableULongModel
    {
        [TpsField("Price")]
        public ulong? Price { get; set; }
    }

    public class DecimalByteModel
    {
        [TpsField("Price")]
        public byte Price { get; set; }
    }

    public class DecimalNullableByteModel
    {
        [TpsField("Price")]
        public byte? Price { get; set; }
    }

    public class DecimalSByteModel
    {
        [TpsField("Price")]
        public sbyte Price { get; set; }
    }

    public class DecimalNullableSByteModel
    {
        [TpsField("Price")]
        public sbyte? Price { get; set; }
    }
}
