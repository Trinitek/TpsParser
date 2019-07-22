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
        [TpsStringField("Price")]
        public string Price { get; set; }
    }

    public class DecimalBooleanModel
    {
        [TpsField("Price")]
        public bool Price { get; set; }
    }

    public class DecimalTpsBooleanFieldModel
    {
        [TpsBooleanField("Price")]
        public bool Price { get; set; }
    }
}
