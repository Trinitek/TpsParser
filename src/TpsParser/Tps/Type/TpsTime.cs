using System;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a moment in time. Some time keeping fields you expect to be of type <see cref="TpsTime"/> may actually be of type <see cref="TpsLong"/>.
    /// See the remarks section for details.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A TIME is composed of 4 bytes. Each field corresponds to a byte as follows:
    /// <list type="table">
    /// <listheader>
    /// <term>Field</term>
    /// <term>Range</term>
    /// </listheader>
    /// <item>
    /// <term>Centiseconds</term>
    /// <description>0 to 99</description>
    /// </item>
    /// <item>
    /// <term>Seconds</term>
    /// <description>0 to 59</description>
    /// </item>
    /// <item>
    /// <term>Minutes</term>
    /// <description>0 to 59</description>
    /// </item>
    /// <item>
    /// <term>Hours</term>
    /// <description>0 to 23</description>
    /// </item>
    /// </list>
    /// A centisecond is 1/100th of a second.
    /// </para>
    /// <para>
    /// In the Clarion programming language, when a TIME value is used in an expression, it is implicitly converted to a
    /// LONG (see <see cref="TpsLong"/>). This is called a Clarion Standard Time value. This value is the number of centiseconds since midnight.
    /// </para>
    /// <para>
    /// Clarion documentation recommends that the 4-byte TIME type be used when communicating with external applications. However, because
    /// TopSpeed files are typically only used by Clarion applications exclusively, the field may sometimes be defined as a LONG in
    /// order to avoid repetitive casting.
    /// </para>
    /// </remarks>
    public sealed class TpsTime : TpsObject<TimeSpan>
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.Time;

        /// <summary>
        /// Instantiates a new TIME from the given value.
        /// </summary>
        /// <param name="time"></param>
        public TpsTime(TimeSpan time)
        {
            Value = time;
        }

        /// <summary>
        /// Returns true if the value is not equal to <see cref="TimeSpan.Zero"/>.
        /// </summary>
        public override Maybe<bool> ToBoolean() => new Maybe<bool>(Value != TimeSpan.Zero);
    }
}
