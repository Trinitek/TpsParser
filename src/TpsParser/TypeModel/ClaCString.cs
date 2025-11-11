using System;
using System.Collections.Generic;

namespace TpsParser.TypeModel;

/// <summary>
/// Represents a Clarion <c>CSTRING</c> type, which is a null-terminated string.
/// </summary>
public readonly struct ClaCString : IClaString, IEquatable<ClaCString>
{
    /// <inheritdoc/>
    public FieldTypeCode TypeCode => FieldTypeCode.CString;

    /// <summary>
    /// Gets the string backing this type.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Instantiates a new <c>CSTRING</c>.
    /// </summary>
    /// <param name="value">The string value. Must not be null.</param>
    public ClaCString(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Returns <see langword="true"/> if the string's length is greater than zero and is not entirely filled with padding whitespace.
    /// </summary>
    // TODO test coverage
    public bool ToBoolean() => Value.Length > 0 && Value.Trim(' ').Length > 0;


    /// <inheritdoc/>
    public override string ToString() => Value;

    /// <inheritdoc/>
    public bool Equals(ClaCString other) =>
        Value == other.Value;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ClaCString x && Equals(x);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return -1937169414 + EqualityComparer<string>.Default.GetHashCode(Value);
    }

    /// <inheritdoc/>
    public static bool operator ==(ClaCString left, ClaCString right) => left.Equals(right);

    /// <inheritdoc/>
    public static bool operator !=(ClaCString left, ClaCString right) => !(left == right);
}
