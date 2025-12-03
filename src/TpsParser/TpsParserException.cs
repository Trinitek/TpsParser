using System;

namespace TpsParser;

/// <summary>
/// Represents errors that occur when reading or parsing a TPS file.
/// </summary>
public class TpsParserException : Exception
{
    /// <inheritdoc/>
    public TpsParserException(string message)
        : base(message)
    { }

    /// <inheritdoc/>
    public TpsParserException(string message, Exception innerException)
        : base(message, innerException)
    { }
}
