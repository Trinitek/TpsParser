using System;

namespace TpsParser
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class TpsFieldAttribute : Attribute
    {
        public string FieldName { get; }

        public TpsFieldAttribute(string fieldName)
        {
            FieldName = fieldName;
        }
    }
}
