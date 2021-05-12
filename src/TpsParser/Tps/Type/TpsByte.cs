using System.Globalization;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a byte.
    /// </summary>
    public sealed class TpsByte : TpsObject<byte>
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.Byte;

        /// <summary>
        /// Instantiates a new BYTE.
        /// </summary>
        /// <param name="value"></param>
        public TpsByte(byte value) => Value = value;

        /// <summary>
        /// Returns true if the value is not zero.
        /// </summary>
        public override Maybe<bool> ToBoolean() => new Maybe<bool>(Value != 0);

        /// <inheritdoc/>
        public override Maybe<sbyte> ToSByte() => new Maybe<sbyte>((sbyte)Value);

        /// <inheritdoc/>
        public override Maybe<byte> ToByte() => new Maybe<byte>(Value);

        /// <inheritdoc/>
        public override Maybe<ushort> ToUInt16() => new Maybe<ushort>(Value);

        /// <inheritdoc/>
        public override Maybe<short> ToInt16() => new Maybe<short>((short)Value);

        /// <inheritdoc/>
        public override Maybe<uint> ToUInt32() => new Maybe<uint>(Value);

        /// <inheritdoc/>
        public override Maybe<int> ToInt32() => new Maybe<int>(Value);

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
