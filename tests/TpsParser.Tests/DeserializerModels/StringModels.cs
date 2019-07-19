namespace TpsParser.Tests.DeserializerModels
{
    public class StringModel
    {
        [TpsField("Notes")]
        public string Notes { get; set; }
    }

    public class StringTrimmingEnabledModel
    {
        [TpsStringField("Notes")]
        public string Notes { get; set; }
    }

    public class StringTrimmingDisabledModel
    {
        [TpsStringField("Notes", TrimEnd = false)]
        public string Notes { get; set; }
    }

    public class StringBooleanTpsFieldAttributeModel
    {
        [TpsField("Notes")]
        public bool HasNotes { get; set; }
    }

    public class StringBooleanModel
    {
        [TpsBooleanField("Notes")]
        public bool HasNotes { get; set; }
    }

    public class StringBooleanTrueModel
    {
        [TpsBooleanField("Notes", TrueValue = "Y")]
        public bool HasNotes { get; set; }
    }

    public class StringBooleanFalseModel
    {
        [TpsBooleanField("Notes", FalseValue = "N")]
        public bool HasNotes { get; set; }
    }

    public class StringBooleanTrueFalseModel
    {
        [TpsBooleanField("Notes", TrueValue = "Y", FalseValue = "N")]
        public bool HasNotes { get; set; }
    }

    public class StringBooleanTrueFalseFallbackFalseModel
    {
        [TpsBooleanField("Notes", TrueValue = "Y", FalseValue = "N", FallbackValue = false)]
        public bool HasNotes { get; set; }
    }

    public class StringBooleanTrueFalseFallbackTrueModel
    {
        [TpsBooleanField("Notes", TrueValue = "Y", FalseValue = "N", FallbackValue = true)]
        public bool HasNotes { get; set; }
    }
}
