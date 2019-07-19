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
        [TpsStringField("Date")]
        public string Date { get; set; }
    }

    public class DateStringFormattedModel
    {
        [TpsStringField("Date", stringFormat: "MM - dd - yyyy")]
        public string Date { get; set; }
    }

    public class DateStringNonStringMemberModel
    {
        [TpsStringField("Date")]
        public int Date { get; set; }
    }

    public class DateStringFallbackModel
    {
        [TpsStringField("Date", fallbackValue: "nothing")]
        public string Date { get; set; }
    }
}
