using System;

namespace TpsParser.TypeModel;

/// <summary>
/// Represents a Clarion MEMO type, which contains a string encoded using the Windows ANSI codepage of the machine on which the
/// text was written. MEMOs are variable-length text fields that can be up to 65,536 bytes long.
/// </summary>
public sealed class TpsMemo : ITpsObject
{
    /// <inheritdoc/>
    public TpsTypeCode TypeCode => TpsTypeCode.None;

    /// <summary>
    /// Gets the string backing this type.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Instantiates a new MEMO.
    /// </summary>
    /// <param name="value"></param>
    public TpsMemo(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <inheritdoc/>
    public override string ToString() => Value;
}
