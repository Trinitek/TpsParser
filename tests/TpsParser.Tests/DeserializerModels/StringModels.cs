namespace TpsParser.Tests.DeserializerModels
{
    public class StringModel
    {
        [TpsField("Notes")]
        public string Notes { get; set; }
    }

    public class StringTrimmingEnabledModel
    {
        [TpsFieldString("Notes")]
        public string Notes { get; set; }
    }

    public class StringTrimmingDisabledModel
    {
        [TpsFieldString("Notes", trimEnd: false)]
        public string Notes { get; set; }
    }
}
