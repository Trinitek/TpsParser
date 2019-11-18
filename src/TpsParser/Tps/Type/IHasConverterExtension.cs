namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Marks the implementing TpsObject as owning additional <see cref="IConvertible{T}"/> implementations
    /// that conflict with the implementations on the class due to inadequate type constraints.
    /// </summary>
    public interface IHasConverterExtension
    {
        /// <summary>
        /// Gets the additional converter extension.
        /// </summary>
        IConverterExtension ConverterExtension { get; }
    }


    /// <summary>
    /// Marks a class that contains additional <see cref="IConvertible{T}"/> implementations.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "Usage as a marker interface is required for the consumer's implementation.")]
    public interface IConverterExtension
    { }
}
