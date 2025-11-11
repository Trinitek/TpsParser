using System;
using System.Collections.Generic;

namespace TpsParser.TypeModel;

/// <summary>
/// Represents a Clarion <c>PSTRING</c> type, which is a Pascal string where the length is specified at the beginning of the string.
/// </summary>
public readonly struct ClaPString : IClaString, IEquatable<ClaPString>
{
    /// <inheritdoc/>
    public FieldTypeCode TypeCode => FieldTypeCode.PString;

    /// <summary>
    /// Gets the string backing this type.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Instantiates a new <c>PSTRING</c>.
    /// </summary>
    /// <param name="value">The string value. Must not be null.</param>
    public ClaPString(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Returns <see langword="true"/> if the string length is greater than zero and is not filled entirely by padding whitespace.
    /// </summary>
    public bool ToBoolean() => Value.Length > 0 && Value.Trim(' ').Length > 0;

    /// <inheritdoc/>
    public override string ToString() => Value;

    /// <inheritdoc/>
    public bool Equals(ClaPString other) =>
        Value == other.Value;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ClaPString x && Equals(x);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return -1937169414 + EqualityComparer<string>.Default.GetHashCode(Value);
    }

    /// <inheritdoc/>
    public static bool operator ==(ClaPString left, ClaPString right) => left.Equals(right);

    /// <inheritdoc/>
    public static bool operator !=(ClaPString left, ClaPString right) => !(left == right);
}
