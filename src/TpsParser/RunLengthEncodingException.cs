using System;

namespace TpsParser
{
    /// <summary>
    /// The exception that is thrown when <see cref="TpsReader"/> encounters an error when decoding an RLE-encoded chunk of data.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1032:Implement standard exception constructors", Justification = "Empty constructor is not used.")]
#if NET45
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2237:Mark ISerializable types with serializable", Justification = "ISerializable is used by binary serialization and is deprecated.")]
#endif
    public sealed class RunLengthEncodingException : Exception
    {
        internal RunLengthEncodingException(string message)
            : base(message)
        { }

        internal RunLengthEncodingException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
