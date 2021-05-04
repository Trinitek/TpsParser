namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents an unsigned short.
    /// </summary>
    public sealed class TpsUnsignedShort : TpsObject<ushort>
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.UShort;

        /// <summary>
        /// Instantiates a new USHORT.
        /// </summary>
        /// <param name="value"></param>
        public TpsUnsignedShort(ushort value) => Value = value;

        /// <summary>
        /// Returns true if the value is not zero.
        /// </summary>
        public override Maybe<bool> ToBoolean() => new Maybe<bool>(Value != 0);
    }
}
