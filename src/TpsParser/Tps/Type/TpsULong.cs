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
    }
}
