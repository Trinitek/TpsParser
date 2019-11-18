namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Defines a method to convert the underlying value to a value of a different type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IConvertible<out T>
    {
        /// <summary>
        /// Gets the underlying value as type <typeparamref name="T"/>.
        /// </summary>
        T AsType();
    }
}
