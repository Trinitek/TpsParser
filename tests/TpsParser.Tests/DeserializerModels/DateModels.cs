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
        [StringOptions("Date")]
        public string Date { get; set; }
    }

    public class DateStringFormattedModel
    {
        [StringOptions("Date", StringFormat = "MM - dd - yyyy")]
        public string Date { get; set; }
    }

    public class DateStringNonStringMemberModel
    {
        [StringOptions("Date")]
        public int Date { get; set; }
    }

    public class DateStringFallbackModel
    {
        [StringOptions("Date", FallbackValue = "nothing")]
        public string Date { get; set; }
    }
}
