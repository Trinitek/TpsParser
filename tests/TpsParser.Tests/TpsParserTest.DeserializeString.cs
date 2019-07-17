namespace TpsParser.Tests
{
    public partial class TpsParserTest
    {
        public class DeserializeString
        {
            [TpsField("Notes")]
            public string Notes { get; set; }
        }

        public class DeserializeStringTrimmingEnabled
        {
            [TpsFieldString("Notes")]
            public string Notes { get; set; }
        }

        public class DeserializeStringTrimmingDisabled
        {
            [TpsFieldString("Notes", trimEnd: false)]
            public string Notes { get; set; }
        }
    }
}
