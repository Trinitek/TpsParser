namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a signed short.
    /// </summary>
    public sealed class TpsShort : TpsObject<short>
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.Short;

        /// <summary>
        /// Instantiates a new SHORT.
        /// </summary>
        /// <param name="value"></param>
        public TpsShort(short value) => Value = value;

        /// <summary>
        /// Returns true if the value is not zero.
        /// </summary>
        public override Maybe<bool> ToBoolean() => new Maybe<bool>(Value != 0);
    }
}
