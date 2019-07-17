using System;
using TpsParser.Binary;

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
        /// Instantiates a new LONG from the given binary reader.
        /// </summary>
        /// <param name="rx"></param>
        public TpsLong(RandomAccess rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            Value = rx.LongLE();
        }

        /// <summary>
        /// Instantiates a new LONG value from the given value.
        /// </summary>
        /// <param name="value"></param>
        public TpsLong(int value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets a <see cref="DateTime"/> representing December 28, 1800, which is the start date of the Clarion Standard Date.
        /// </summary>
        public static readonly DateTime ClarionEpoch = new DateTime(1800, 12, 28);

        /// <summary>
        /// Gets a <see cref="DateTime"/> by treating the value as a Clarion Standard Date, where the value is the number of days since <see cref="ClarionEpoch"/>.
        /// For more information about the Clarion Standard Date, see the remarks section of <see cref="TpsDate"/>.
        /// </summary>
        /// <returns></returns>
        public DateTime AsDate() => ClarionEpoch.AddDays(Value);

        /// <summary>
        /// Gets a <see cref="TimeSpan"/> by treating the value as a Clarion Standard Time, where the value is the number of centiseconds (1/100 seconds) since midnight.
        /// For more information about the Clarion Standard Time, see the remarks section of <see cref="TpsTime"/>.
        /// </summary>
        /// <returns></returns>
        public TimeSpan AsTime() => new TimeSpan(0, 0, 0, 0, Value * 10);
    }
}
