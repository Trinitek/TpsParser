using System;

namespace TpsParser.Tests
{
    public partial class TpsParserTest
    {
        [TpsTable]
        public class DeserializeTime
        {
            [TpsField("Time")]
            public TimeSpan Time { get; set; }
        }
    }
}
