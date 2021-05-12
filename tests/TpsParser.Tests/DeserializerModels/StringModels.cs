namespace TpsParser.Tests.DeserializerModels
{
    public class StringModel
    {
        [TpsField("Notes")]
        public string Notes { get; set; }
    }

    public class StringTrimmingEnabledModel
    {
        [StringOptions("Notes")]
        public string Notes { get; set; }
    }

    public class StringTrimmingDisabledModel
    {
        [StringOptions("Notes", TrimEnd = false)]
        public string Notes { get; set; }
    }

    public class StringBooleanTpsFieldAttributeModel
    {
        [TpsField("Notes")]
        public bool HasNotes { get; set; }
    }

    public class StringBooleanModel
    {
        [BooleanOptions("Notes")]
        public bool HasNotes { get; set; }
    }

    public class StringBooleanTrueModel
    {
        [BooleanOptions("Notes", TrueValue = "Y")]
        public bool HasNotes { get; set; }
    }

    public class StringBooleanFalseModel
    {
        [BooleanOptions("Notes", FalseValue = "N")]
        public bool HasNotes { get; set; }
    }

    public class StringBooleanTrueFalseModel
    {
        [BooleanOptions("Notes", TrueValue = "Y", FalseValue = "N")]
        public bool HasNotes { get; set; }
    }

    public class StringBooleanTrueFalseFallbackFalseModel
    {
        [BooleanOptions("Notes", TrueValue = "Y", FalseValue = "N", FallbackValue = false)]
        public bool HasNotes { get; set; }
    }

    public class StringBooleanTrueFalseFallbackTrueModel
    {
        [BooleanOptions("Notes", TrueValue = "Y", FalseValue = "N", FallbackValue = true)]
        public bool HasNotes { get; set; }
    }
}
