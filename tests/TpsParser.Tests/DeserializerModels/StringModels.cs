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
        [TpsStringField("Notes", trimEnd: false)]
        public string Notes { get; set; }
    }
}
