using System;

namespace TpsParser;

/// <summary>
/// Represents errors that occur when decoding run-length encoded data.
/// </summary>
public sealed class RunLengthEncodingException : TpsParserException
{
    /// <inheritdoc/>
    public RunLengthEncodingException(string message)
        : base(message)
    { }

    /// <inheritdoc/>
    public RunLengthEncodingException(string message, Exception innerException)
        : base(message, innerException)
    { }
}
