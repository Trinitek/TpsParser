using System;

namespace TpsParser
{
    /// <summary>
    /// <para>
    /// When present on a property or field, the value will be set to the row number for that particular record.
    /// </para>
    /// <para>
    /// If present on a field, the field may be private.
    /// </para>
    /// <para>
    /// If present on a property, the property must have a setter. The setter may be private.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class TpsRecordNumberAttribute : Attribute
    { }
}
