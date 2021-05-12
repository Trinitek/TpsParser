﻿using System;
using System.Globalization;

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
        /// <summary>
        /// Gets a <see cref="DateTime"/> representing December 28, 1800, which is the start date of the Clarion Standard Date.
        /// </summary>
        public static DateTime ClarionEpoch => new DateTime(1800, 12, 28);

        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.Date;

        /// <summary>
        /// Instantiates a new DATE.
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
        public override Maybe<bool> ToBoolean() => new Maybe<bool>(Value != null);

        /// <inheritdoc/>
        public override Maybe<DateTime?> ToDateTime() => new Maybe<DateTime?>(Value);

        /// <inheritdoc/>
        public override string ToString(string format) => Value?.ToString(format, CultureInfo.InvariantCulture);
    }
}
