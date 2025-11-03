using System;

namespace TpsParser.TypeModel;

/// <summary>
/// Represents a Clarion BLOB type, which is an arbitrary array of bytes. Unlike <see cref="TpsMemo"/>,
/// a BLOB can be larger than 65,536 bytes.
/// </summary>
public sealed class TpsBlob : IClaObject
{
    /// <inheritdoc/>
    public ClaTypeCode TypeCode => ClaTypeCode.None;

    /// <summary>
    /// Gets the byte array backing this type.
    /// </summary>
    public ReadOnlyMemory<byte> Value { get; }

    /// <summary>
    /// Instantiates a new BLOB.
    /// </summary>
    /// <param name="value"></param>
    public TpsBlob(ReadOnlyMemory<byte> value)
    {
        Value = value;
    }
}
