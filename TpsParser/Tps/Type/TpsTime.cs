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
            // Currently only knows how to handle hours and minutes, but not seconds or milliseconds.

            int time = rx.LongLE();

            int mins = (time & 0x00FF0000) >> 16;
            int hours = (time & 0x7F000000) >> 24;

            Value = new TimeSpan(hours, mins, 0);
        }
    }
}
