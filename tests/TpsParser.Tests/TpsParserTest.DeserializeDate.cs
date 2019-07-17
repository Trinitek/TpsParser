using System;

namespace TpsParser.Tests
{
    public partial class TpsParserTest
    {
        [TpsTable]
        public class DeserializeDate
        {
            [TpsField("Date")]
            public DateTime Date { get; set; }
        }

        [TpsTable]
        public class DeserializeNullDate
        {
            [TpsField("Date")]
            public DateTime? Date { get; set; }
        }
    }
}
