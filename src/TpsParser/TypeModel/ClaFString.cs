using System;
using System.Collections.Generic;

namespace TpsParser.TypeModel;

/// <summary>
/// Represents a Clarion <c>STRING</c> type, which is a fixed-length string.
/// </summary>
public readonly struct ClaFString : IClaString, IEquatable<ClaFString>
{
    /// <inheritdoc/>
    public ClaTypeCode TypeCode => ClaTypeCode.FString;

    /// <summary>
    /// Gets the string backing this type.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Instantiates a new <c>STRING</c>.
    /// </summary>
    /// <param name="value">The string value. Must not be null.</param>
    public ClaFString(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Returns <see langword="true"/> if the string's length is greater than zero and is not entirely filled with padding whitespace.
    /// </summary>
    /// <returns></returns>
    public bool ToBoolean() => Value.Length > 0 && Value.Trim(' ').Length > 0;

    /// <inheritdoc/>
    public override string ToString() => Value;

    /// <inheritdoc/>
    public bool Equals(ClaFString other) =>
        Value == other.Value;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ClaFString x && Equals(x);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return -1937169414 + EqualityComparer<string>.Default.GetHashCode(Value);
    }

    /// <inheritdoc/>
    public static bool operator ==(ClaFString left, ClaFString right) => EqualityComparer<ClaFString>.Default.Equals(left, right);

    /// <inheritdoc/>
    public static bool operator !=(ClaFString left, ClaFString right) => !(left == right);
}
