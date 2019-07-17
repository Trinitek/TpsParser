using System;

namespace TpsParser.Tests
{
    public partial class TpsParserTest
    {
        public class DeserializeDate
        {
            [TpsField("Date")]
            public DateTime Date { get; set; }
        }

        public class DeserializeNullDate
        {
            [TpsField("Date")]
            public DateTime? Date { get; set; }
        }

        public class DeserializeDateString
        {
            [TpsFieldString("Date")]
            public string Date { get; set; }
        }

        public class DeserializeDateStringFormatted
        {
            [TpsFieldString("Date", stringFormat: "MM - dd - yyyy")]
            public string Date { get; set; }
        }

        public class DeserializeDateStringNonStringMember
        {
            [TpsFieldString("Date")]
            public int Date { get; set; }
        }

        public class DeserializeDateStringFallback
        {
            [TpsFieldString("Date", fallbackValue: "nothing")]
            public string Date { get; set; }
        }
    }
}
