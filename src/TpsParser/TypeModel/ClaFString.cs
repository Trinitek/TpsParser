using System;
using System.Collections.Generic;

namespace TpsParser.TypeModel;

/// <summary>
/// Represents a Clarion STRING type, which is a fixed-length string.
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
    /// Instantiates a new STRING.
    /// </summary>
    /// <param name="value">The string value. Must not be null.</param>
    public ClaFString(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <inheritdoc/>
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
