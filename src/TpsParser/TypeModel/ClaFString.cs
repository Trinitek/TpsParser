using System;
using System.Collections.Generic;
using System.Text;

namespace TpsParser.TypeModel;

/// <summary>
/// Represents a Clarion <c>STRING</c> type, which is a fixed-length string.
/// </summary>
public readonly struct ClaFString : IClaString, IEquatable<ClaFString>
{
    /// <inheritdoc/>
    public FieldTypeCode TypeCode => FieldTypeCode.FString;

    /// <inheritdoc cref="IClaString.StringValue"/>
    public string? StringValue { get; }

    /// <inheritdoc cref="IClaString.ContentValue"/>
    public ReadOnlyMemory<byte>? ContentValue { get; }

    /// <summary>
    /// Instantiates a new <c>STRING</c>.
    /// </summary>
    /// <param name="value">The string value. Must not be null.</param>
    public ClaFString(string value)
    {
        StringValue = value ?? throw new ArgumentNullException(nameof(value));
        ContentValue = null;
    }

    /// <summary>
    /// Instantiates a new <c>STRING</c>.
    /// </summary>
    /// <param name="value">The memory section that contains the string value.</param>
    public ClaFString(ReadOnlyMemory<byte> value)
    {
        ContentValue = value;
        StringValue = null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the string's length is greater than zero and is not entirely filled with SPACE as padding.
    /// If <see cref="StringValue"/> is not available, <see cref="ContentValue"/> is used and
    /// the SPACE character is determined using the default <see cref="EncodingOptions.ContentEncoding"/> set by <see cref="EncodingOptions.Default"/>.
    /// </summary>
    /// <returns></returns>
    public bool ToBoolean()
    {
        return ToBoolean(EncodingOptions.Default.ContentEncoding);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the string's length is greater than zero and is not entirely filled with SPACE as padding.
    /// If <see cref="StringValue"/> is available, <paramref name="encoding"/> is ignored.
    /// Otherwise, the SPACE character is determined using the specified encoding against <see cref="ContentValue"/>.
    /// </summary>
    /// <param name="encoding"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public bool ToBoolean(Encoding encoding)
    {
        return ClaStringCommon.ToBoolean(StringValue, ContentValue, encoding);
    }

    /// <summary>
    /// Gets the string value stored in <see cref="StringValue"/> if available,
    /// or returns a new string from <see cref="ContentValue"/> using the default
    /// <see cref="EncodingOptions.ContentEncoding"/> set by <see cref="EncodingOptions.Default"/>.
    /// </summary>
    public override string ToString()
    {
        return ToString(EncodingOptions.Default.ContentEncoding);
    }

    /// <inheritdoc/>
    public string ToString(Encoding encoding)
    {
        return ClaStringCommon.ToString(StringValue, ContentValue, encoding);
    }

    /// <inheritdoc/>
    public bool Equals(ClaFString other) =>
        ClaStringCommon.Equals(
            StringValue,
            ContentValue,
            other.StringValue,
            other.ContentValue);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ClaFString x && Equals(x);

    /// <inheritdoc/>
    public override int GetHashCode() =>
        ClaStringCommon.GetHashCode(
            seed: -1937169414,
            StringValue,
            ContentValue);

    /// <inheritdoc/>
    public static bool operator ==(ClaFString left, ClaFString right) => EqualityComparer<ClaFString>.Default.Equals(left, right);

    /// <inheritdoc/>
    public static bool operator !=(ClaFString left, ClaFString right) => !(left == right);
}
