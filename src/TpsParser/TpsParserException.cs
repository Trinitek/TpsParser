using System;

namespace TpsParser
{
    public sealed class TpsParserException : Exception
    {
        public TpsParserException(string message)
            : base(message)
        { }

        public TpsParserException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
