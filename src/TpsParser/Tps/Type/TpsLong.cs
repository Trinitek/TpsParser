using System;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a signed integer.
    /// </summary>
    public sealed class TpsLong : TpsObject<int>
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.Long;

        /// <summary>
        /// Instantiates a new LONG value from the given value.
        /// </summary>
        /// <param name="value"></param>
        public TpsLong(int value)
        {
            Value = value;
        }

        /// <summary>
        /// Returns true if the value is not zero.
        /// </summary>
        public override Maybe<bool> ToBoolean() => new Maybe<bool>(Value != 0);

        /// <summary>
        /// Gets a <see cref="DateTime"/> by treating the value as a Clarion Standard Date, where the value is the number of days since <see cref="TpsDate.ClarionEpoch"/>.
        /// For more information about the Clarion Standard Date, see the remarks section of <see cref="TpsDate"/>.
        /// </summary>
        public override Maybe<DateTime?> ToDateTime() => new Maybe<DateTime?>(TpsDate.ClarionEpoch.AddDays(Value));

        /// <summary>
        /// Gets a <see cref="TimeSpan"/> by treating the value as a Clarion Standard Time, where the value is the number of centiseconds (1/100 seconds) since midnight.
        /// For more information about the Clarion Standard Time, see the remarks section of <see cref="TpsTime"/>.
        /// </summary>
        public override Maybe<TimeSpan> ToTimeSpan() => new Maybe<TimeSpan>(new TimeSpan(0, 0, 0, 0, Value * 10));
    }
}
