namespace TpsParser.Tests.DeserializerModels
{
    public static class DeserializerModel
    {
        public const string FieldName = "F";
    }

    public class DeserializerModel<T>
    {
        [TpsField(DeserializerModel.FieldName)]
        public T Value { get; set; }
    }

    public class TpsStringFieldDeserializerModel
    {
        [TpsStringField(DeserializerModel.FieldName)]
        public string Value { get; set; }
    }

    public class TpsBooleanFieldDeserializerModel
    {
        [TpsBooleanField(DeserializerModel.FieldName)]
        public bool Value { get; set; }
    }
}
