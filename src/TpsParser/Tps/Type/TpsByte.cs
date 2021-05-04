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
    }
}
