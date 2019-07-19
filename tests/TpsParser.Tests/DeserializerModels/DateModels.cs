using System;

namespace TpsParser.Tests.DeserializerModels
{
    public class DateModel
    {
        [TpsField("Date")]
        public DateTime Date { get; set; }
    }

    public class NullDateModel
    {
        [TpsField("Date")]
        public DateTime? Date { get; set; }
    }

    public class DateStringModel
    {
        [TpsFieldString("Date")]
        public string Date { get; set; }
    }

    public class DateStringFormattedModel
    {
        [TpsFieldString("Date", stringFormat: "MM - dd - yyyy")]
        public string Date { get; set; }
    }

    public class DateStringNonStringMemberModel
    {
        [TpsFieldString("Date")]
        public int Date { get; set; }
    }

    public class DateStringFallbackModel
    {
        [TpsFieldString("Date", fallbackValue: "nothing")]
        public string Date { get; set; }
    }
}
