using System;

namespace TpsParser
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class TpsRecordNumberAttribute : Attribute
    { }
}
