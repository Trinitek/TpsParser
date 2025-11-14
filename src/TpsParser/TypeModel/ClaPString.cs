using System;
using System.Collections.Generic;
using System.Text;

namespace TpsParser.TypeModel;

/// <summary>
/// Represents a Clarion <c>PSTRING</c> type, which is a Pascal string where the length is specified at the beginning of the string.
/// </summary>
public readonly struct ClaPString : IClaString, IEquatable<ClaPString>
{
    /// <inheritdoc/>
    public FieldTypeCode TypeCode => FieldTypeCode.PString;

    /// <inheritdoc/>
    public string? StringValue { get; }

    /// <inheritdoc/>
    public ReadOnlyMemory<byte>? ContentValue { get; }

    /// <summary>
    /// Instantiates a new <c>PSTRING</c>.
    /// </summary>
    /// <param name="value">The string value. Must not be null.</param>
    public ClaPString(string value)
    {
        StringValue = value ?? throw new ArgumentNullException(nameof(value));
        ContentValue = null;
    }

    /// <summary>
    /// Instantiates a new <c>PSTRING</c>.
    /// </summary>
    /// <param name="value">The memory section that contains the string value.</param>
    public ClaPString(ReadOnlyMemory<byte> value)
    {
        StringValue = null;
        ContentValue = value;
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
        if (StringValue is not null)
        {
            return StringValue;
        }

        var contentSpan = ContentValue!.Value.Span;

        if (contentSpan.Length == 0)
        {
            return string.Empty;
        }

        byte length = contentSpan[0];

        return encoding.GetString(contentSpan[1..length]);
    }

    /// <inheritdoc/>
    public bool Equals(ClaPString other) =>
        ClaStringCommon.Equals(
            StringValue,
            ContentValue,
            other.StringValue,
            other.ContentValue);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ClaPString x && Equals(x);

    /// <inheritdoc/>
    public override int GetHashCode() =>
        ClaStringCommon.GetHashCode(
            seed: -1937169414,
            StringValue,
            ContentValue);

    /// <inheritdoc/>
    public static bool operator ==(ClaPString left, ClaPString right) => left.Equals(right);

    /// <inheritdoc/>
    public static bool operator !=(ClaPString left, ClaPString right) => !(left == right);
}
