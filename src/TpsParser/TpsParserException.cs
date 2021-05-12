using System;

namespace TpsParser
{
    /// <summary>
    /// The exception that is thrown when the parser experiences any type of error.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "Empty constructor is not used.")]
#if NET45
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2237:Mark ISerializable types with serializable", Justification = "ISerializable is used by binary serialization and is deprecated.")]
#endif
    public sealed class TpsParserException : Exception
    {
        internal TpsParserException(string message)
            : base(message)
        { }

        internal TpsParserException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
