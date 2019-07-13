using System;

namespace TpsParser.Tps
{
    public sealed class NotATopSpeedFileException : Exception
    {
        public NotATopSpeedFileException(string message)
            : base(message)
        { }
    }
}
