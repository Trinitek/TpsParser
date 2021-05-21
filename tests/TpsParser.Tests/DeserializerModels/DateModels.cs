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
        [TpsField("Date")]
        public string Date { get; set; }
    }

    public class DateStringFormattedModel
    {
        [TpsField("Date")]
        [StringOptions(StringFormat = "MM - dd - yyyy")]
        public string Date { get; set; }
    }

    public class DateStringNonStringMemberModel
    {
        [TpsField("Date")]
        public int Date { get; set; }
    }

    public class DateStringFallbackModel
    {
        [TpsField("Date", FallbackValue = "nothing")]
        public string Date { get; set; }
    }
}
