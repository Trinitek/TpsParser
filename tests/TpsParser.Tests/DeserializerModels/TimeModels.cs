using System;

namespace TpsParser.Tests.DeserializerModels
{
    public class TimeModel
    {
        [TpsField("Time")]
        public TimeSpan Time { get; set; }
    }
}
