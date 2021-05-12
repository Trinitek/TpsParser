using System.Globalization;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents an unsigned integer.
    /// </summary>
    public sealed class TpsUnsignedLong : TpsObject<uint>
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.ULong;

        /// <summary>
        /// Instantiates a new ULONG.
        /// </summary>
        /// <param name="value"></param>
        public TpsUnsignedLong(uint value)
        {
            Value = value;
        }

        /// <summary>
        /// Returns true if the value is not zero.
        /// </summary>
        public override Maybe<bool> ToBoolean() => new Maybe<bool>(Value != 0);

        /// <inheritdoc/>
        public override Maybe<uint> ToUInt32() => new Maybe<uint>(Value);

        /// <inheritdoc/>
        public override Maybe<int> ToInt32() => new Maybe<int>((int)Value);

        /// <inheritdoc/>
        public override Maybe<ulong> ToUInt64() => new Maybe<ulong>(Value);

        /// <inheritdoc/>
        public override Maybe<long> ToInt64() => new Maybe<long>(Value);

        /// <inheritdoc/>
        public override Maybe<decimal> ToDecimal() => new Maybe<decimal>(Value);

        /// <inheritdoc/>
        public override string ToString(string format) => Value.ToString(format, CultureInfo.InvariantCulture);
    }
}
