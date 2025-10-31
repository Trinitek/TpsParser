using System;
using System.Collections.Generic;

namespace TpsParser.TypeModel;

/// <summary>
/// Represents an arbitrary array of bytes. Unlike <see cref="TpsMemo"/>,
/// a BLOB can be larger than 65,536 bytes.
/// </summary>
public sealed class TpsBlob : ITpsObject
{
    /// <inheritdoc/>
    public TpsTypeCode TypeCode => TpsTypeCode.None;

    /// <summary>
    /// Gets the byte array backing this type.
    /// </summary>
    public IReadOnlyList<byte> Value => _value;
    private readonly byte[] _value;

    /// <summary>
    /// Instantiates a new BLOB.
    /// </summary>
    /// <param name="value"></param>
    public TpsBlob(byte[] value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }
}
