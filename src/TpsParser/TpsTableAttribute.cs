using System;

namespace TpsParser
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TpsTableAttribute : Attribute
    {
        public TpsTableAttribute()
        { }
    }
}
