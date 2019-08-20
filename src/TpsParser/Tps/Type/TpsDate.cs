using System;
using TpsParser.Binary;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a date. Some time keeping fields you expect to be of type <see cref="TpsDate"/>  may actually be of type <see cref="TpsLong"/>.
    /// See the remarks section for details.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A DATE is composed of 4 bytes.
    /// <list type="table">
    /// <listheader>
    /// <term>Field</term>
    /// <term>Range</term>
    /// </listheader>
    /// <item>
    /// <term>Day (byte)</term>
    /// <description>1 to 31</description>
    /// </item>
    /// <item>
    /// <term>Month (byte)</term>
    /// <description>1 to 12</description>
    /// </item>
    /// <item>
    /// <term>Year (word)</term>
    /// <description>1 to 9999</description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// In the Clarion programming language, when a DATE value is used in an expression, it is implicitly converted to a
    /// LONG (<see cref="TpsLong"/>). This is called a Clarion Standard Date value. This value is the number of days since December 28, 1880.
    /// The valid Clarion Standard Date range is January 1, 1801 through December 31, 9999.
    /// </para>
    /// <para>
    /// Clarion documentation recommends that the 4-byte DATE type be used when communicating with external applications. However, because
    /// TopSpeed files are typically only used by Clarion applications exclusively, the field may sometimes be defined as a LONG in
    /// order to avoid repetitive casting.
    /// </para>
    /// </remarks>
    public sealed class TpsDate : TpsObject<DateTime?>
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.Date;

        /// <summary>
        /// Instantiates a new DATE from the given binary reader.
        /// </summary>
        /// <remarks>
        /// The byte stream is read in the following order:
        /// <list type="bullet">
        /// <item>Day</item>
        /// <item>Month</item>
        /// <item>Year (low half)</item>
        /// <item>Year (high half)</item>
        /// </list>
        /// </remarks>
        /// <param name="rx"></param>
        public TpsDate(RandomAccess rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            // Date, mask encoded

            long date = rx.UnsignedLongLE();

            if (date != 0)
            {
                long years = (date & 0xFFFF0000) >> 16;
                long months = (date & 0x0000FF00) >> 8;
                long days = date & 0x000000FF;
                Value = new DateTime((int)years, (int)months, (int)days);
            }
            else
            {
                Value = null;
            }
        }

        /// <summary>
        /// Instantiates a new DATE from the given value.
        /// </summary>
        /// <param name="date"></param>
        public TpsDate(DateTime? date)
        {
            Value = date;
        }

        /// <summary>
        /// Returns true if the date value is not null.
        /// </summary>
        /// <returns></returns>
        protected override bool AsBoolean() => Value != null;
    }
}
