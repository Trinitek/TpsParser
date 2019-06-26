using System;

namespace TpsParser
{
    public sealed class RunLengthEncodingException : Exception
    {
        public RunLengthEncodingException(string message)
            : base(message)
        { }

        public RunLengthEncodingException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
