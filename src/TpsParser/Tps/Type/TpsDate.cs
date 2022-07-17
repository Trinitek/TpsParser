using System;
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
    /// In the Clarion documentation, this type is often referred to as a Btrieve date, referring to the historical
    /// <see href="https://en.wikipedia.org/wiki/Btrieve">Btrieve Record Manager</see> and is designed for interoperability with that and
    /// other external systems.
    /// </para>
    /// <para>
    /// The native date type used in the Clarion programming language when performing calculations is a LONG (<see cref="TpsLong"/>).
    /// This is called a Clarion Standard Date value and counts the number of days since December 28, 1800.
    /// The valid Clarion Standard Date range is January 1, 1801 through December 31, 9999, that is, an inclusive numerical range from 4
    /// to 2,994,626. However, the <see cref="TpsDate"/> type is not subject to this restriction and can represent any date between
    /// 0001-01-01 and 9999-12-31, with 0000-00-00 used to represent a null value. Unlike a Clarion Standard Time (see <see cref="TpsTime"/>),
    /// a Clarion Standard Date does not have a null-equivalent value, and the documentation only says that values outside of the valid range
    /// will yield undefined behavior when used with date functions.
    /// </para>
    /// </remarks>
    public readonly struct TpsDate : IDate, IEquatable<TpsDate>
    {
        /// <summary>
        /// Gets a <see cref="DateTime"/> representing December 28, 1800, which is the reference date for Clarion Standard Date values.
        /// </summary>
        public static readonly DateTime ClarionEpoch = new DateTime(1800, 12, 28);

        /// <summary>
        /// Gets the minimum valid value of a Clarion Standard Date, January 1, 1801. This is 4 days after <see cref="ClarionEpoch"/>.
        /// </summary>
        public static readonly int ClarionStandardDateMinValue = 4;

        /// <summary>
        /// Gets the maximum valid value of a Clarion Standard Date, December 31, 9999.
        /// </summary>
        public static readonly int ClarionStandardDateMaxValue = 2994626;

        /// <inheritdoc/>
        public TpsTypeCode TypeCode => TpsTypeCode.Date;

        private DateTime? Value { get; }

        /// <summary>
        /// Gets the year.
        /// </summary>
        public ushort Year => (ushort)(Value?.Year ?? 0);

        /// <summary>
        /// Gets the month.
        /// </summary>
        public byte Month => (byte)(Value?.Month ?? 0);

        /// <summary>
        /// Gets the day.
        /// </summary>
        public byte Day => (byte)(Value?.Day ?? 0);

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
        public Maybe<bool> ToBoolean() => Maybe.Some(Value != null);

        /// <inheritdoc/>
        public Maybe<DateTime?> ToDateTime() => Maybe.Some(Value);

        /// <summary>
        /// Gets a <see cref="TpsLong"/> instance representing the Clarion Standard Date, or number of days since December 28, 1800.
        /// For dates before January 1, 1801 (four days after <see cref="ClarionEpoch"/>), this returns <see cref="Maybe.None{T}"/>.
        /// </summary>
        /// <returns></returns>
        public Maybe<TpsLong> AsClarionStandardDate() =>
            Value is null || Value < ClarionEpoch.AddDays(ClarionStandardDateMinValue)
            ? Maybe.None<TpsLong>()
            : Maybe.Some(new TpsLong((Value.Value - ClarionEpoch).Days));

        /// <inheritdoc/>
        public override string ToString() => Value?.ToString(CultureInfo.InvariantCulture);

        /// <inheritdoc/>
        public bool Equals(TpsDate other) =>
            Value == other.Value;

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is TpsDate x && Equals(x);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return -1937169414 + Value.GetHashCode();
        }

        /// <inheritdoc/>
        public static bool operator ==(TpsDate left, TpsDate right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(TpsDate left, TpsDate right) => !(left == right);
    }
}
