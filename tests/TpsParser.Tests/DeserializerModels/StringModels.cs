namespace TpsParser.Tests.DeserializerModels
{
    public class StringModel
    {
        [TpsField("Notes")]
        public string Notes { get; set; }
    }

    public class StringTrimmingEnabledModel
    {
        [TpsField("Notes")]
        public string Notes { get; set; }
    }

    public class StringTrimmingDisabledModel
    {
        [TpsField("Notes")]
        [StringOptions(TrimEnd = false)]
        public string Notes { get; set; }
    }

    public class StringBooleanTpsFieldAttributeModel
    {
        [TpsField("Notes")]
        public bool HasNotes { get; set; }
    }

    public class StringBooleanModel
    {
        [TpsField("Notes")]
        public bool HasNotes { get; set; }
    }

    public class StringBooleanTrueModel
    {
        [TpsField("Notes")]
        [BooleanOptions(TrueValue = "Y")]
        public bool HasNotes { get; set; }
    }

    public class StringBooleanFalseModel
    {
        [TpsField("Notes")]
        [BooleanOptions(FalseValue = "N")]
        public bool HasNotes { get; set; }
    }

    public class StringBooleanTrueFalseModel
    {
        [TpsField("Notes")]
        [BooleanOptions(TrueValue = "Y", FalseValue = "N")]
        public bool HasNotes { get; set; }
    }

    public class StringBooleanTrueFalseFallbackFalseModel
    {
        [TpsField("Notes", FallbackValue = false)]
        [BooleanOptions(TrueValue = "Y", FalseValue = "N")]
        public bool HasNotes { get; set; }
    }

    public class StringBooleanTrueFalseFallbackTrueModel
    {
        [TpsField("Notes", FallbackValue = true)]
        [BooleanOptions(TrueValue = "Y", FalseValue = "N")]
        public bool HasNotes { get; set; }
    }
}
