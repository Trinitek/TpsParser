using System;
using TpsParser.Binary;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a moment in time. Some time keeping fields you expect to be of type <see cref="TpsTime"/> may actually be of type <see cref="TpsLong"/>.
    /// See the remarks section for details.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A TIME is composed of 4 bytes. The structure is composed as such:
    /// <list type="table">
    /// <listheader>
    /// <term>Type</term>
    /// <term>Description</term>
    /// <term>Range</term>
    /// </listheader>
    /// <item>
    /// <term><see cref="byte"/></term>
    /// <term>Hours</term>
    /// <term>0 to 23</term>
    /// </item>
    /// <item>
    /// <term><see cref="byte"/></term>
    /// <term>Minutes</term>
    /// <term>0 to 59</term>
    /// </item>
    /// <item>
    /// <term><see cref="byte"/></term>
    /// <term>Seconds</term>
    /// <term>0 to 59</term>
    /// </item>
    /// <item>
    /// <term><see cref="byte"/></term>
    /// <term>Centiseconds (1/100 seconds)</term>
    /// <term>0 to 99</term>
    /// </item>
    /// </list>
    /// A centisecond is 1/100th of a second.
    /// </para>
    /// <para>
    /// In the Clarion programming language, when an expression is performed on a TIME data type, it is implicitly converted to a
    /// LONG as a Clarion Standard Time value. This value is the number of centiseconds (1/100 seconds) since midnight. Clarion
    /// documentation recommends that the 4-byte TIME type be used when communicating with external applications. However, because
    /// TopSpeed files are typically used exclusively by Clarion applications, the field may sometimes be defined as a LONG in
    /// order to avoid repetitive casting.
    /// </para>
    /// </remarks>
    public sealed class TpsTime : TpsObject<TimeSpan>
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.Time;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public TpsTime(RandomAccess rx)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
