using System;
using System.Collections.Generic;
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
    /// LONG (<see cref="TpsLong"/>). This is called a Clarion Standard Date value. This value is the number of days since December 28, 1800.
    /// The valid Clarion Standard Date range is January 1, 1801 through December 31, 9999.
    /// </para>
    /// <para>
    /// Clarion documentation recommends that the 4-byte DATE type be used when communicating with external applications. However, because
    /// TopSpeed files are typically only used by Clarion applications exclusively, the field may sometimes be defined as a LONG in
    /// order to avoid repetitive casting.
    /// </para>
    /// </remarks>
    public readonly struct TpsDate : ITpsObject, IEquatable<TpsDate>
    {
        /// <summary>
        /// Gets a <see cref="DateTime"/> representing December 28, 1800, which is the start date of the Clarion Standard Date.
        /// </summary>
        public static DateTime ClarionEpoch => new DateTime(1800, 12, 28);

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

        /// <inheritdoc/>
        public Maybe<sbyte> ToSByte() => Maybe.None<sbyte>();

        /// <inheritdoc/>
        public Maybe<byte> ToByte() => Maybe.None<byte>();

        /// <inheritdoc/>
        public Maybe<short> ToInt16() => Maybe.None<short>();

        /// <inheritdoc/>
        public Maybe<ushort> ToUInt16() => Maybe.None<ushort>();

        /// <inheritdoc/>
        public Maybe<int> ToInt32() => Maybe.None<int>();

        /// <inheritdoc/>
        public Maybe<uint> ToUInt32() => Maybe.None<uint>();

        /// <inheritdoc/>
        public Maybe<long> ToInt64() => Maybe.None<long>();

        /// <inheritdoc/>
        public Maybe<ulong> ToUInt64() => Maybe.None<ulong>();

        /// <inheritdoc/>
        public Maybe<float> ToFloat() => Maybe.None<float>();

        /// <inheritdoc/>
        public Maybe<double> ToDouble() => Maybe.None<double>();

        /// <inheritdoc/>
        public Maybe<decimal> ToDecimal() => Maybe.None<decimal>();

        /// <inheritdoc/>
        public Maybe<TimeSpan> ToTimeSpan() => Maybe.None<TimeSpan>();

        /// <inheritdoc/>
        public Maybe<IReadOnlyList<ITpsObject>> ToArray() => Maybe.None<IReadOnlyList<ITpsObject>>();

        /// <summary>
        /// Gets a <see cref="TpsLong"/> instance representing the Clarion Standard Date, or number of days since December 28, 1800.
        /// </summary>
        /// <returns></returns>
        public Maybe<TpsLong> AsClarionStandardDate() =>
            Value is null
            ? Maybe.None<TpsLong>()
            : Maybe.Some(new TpsLong((Value.Value - ClarionEpoch).Days));

        /// <inheritdoc/>
        public override string ToString() => Value?.ToString(CultureInfo.InvariantCulture);

        /// <inheritdoc/>
        public string ToString(string format) => Value?.ToString(format, CultureInfo.InvariantCulture);

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is TpsDate x && Equals(x);

        /// <inheritdoc/>
        public bool Equals(TpsDate other) =>
            Value == other.Value;

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
