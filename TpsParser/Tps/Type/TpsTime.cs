using System;
using TpsParser.Binary;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a moment in time.
    /// </summary>
    public sealed class TpsTime : TpsObject<TimeSpan>
    {
        public override TpsTypeCode TypeCode => TpsTypeCode.Time;

        public TpsTime(RandomAccess rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            // Time, mask encoded

            int time = rx.LongLE();

            // Hours 0 - 23
            int hours = (time & 0x7F000000) >> 24;

            // Minutes 0 - 59
            int mins = (time & 0x00FF0000) >> 16;

            // Seconds 0 - 59
            int secs = (time & 0x0000FF00) >> 8;

            // Centiseconds (seconds/100) 0 - 99
            int centi = time & 0x000000FF;

            Value = new TimeSpan(0, hours, mins, secs, centi * 10);
        }
    }
}
