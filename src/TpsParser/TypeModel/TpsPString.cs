using System;
using System.Collections.Generic;

namespace TpsParser.TypeModel;

/// <summary>
/// Represents a Pascal string where the length is specified at the beginning of the string.
/// </summary>
public readonly struct TpsPString : IString, IEquatable<TpsPString>
{
    /// <inheritdoc/>
    public TpsTypeCode TypeCode => TpsTypeCode.PString;

    /// <summary>
    /// Gets the string backing this type.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Instantiate a new PSTRING.
    /// </summary>
    /// <param name="value">The string value. Must not be null.</param>
    public TpsPString(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Returns true if the string length is not zero.
    /// </summary>
    public bool ToBoolean() => Value.Length > 0 && Value.Trim(' ').Length > 0;

    public override string ToString() => Value;

    /// <inheritdoc/>
    public bool Equals(TpsPString other) =>
        Value == other.Value;

    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is TpsPString x && Equals(x);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return -1937169414 + EqualityComparer<string>.Default.GetHashCode(Value);
    }

    /// <inheritdoc/>
    public static bool operator ==(TpsPString left, TpsPString right) => left.Equals(right);

    /// <inheritdoc/>
    public static bool operator !=(TpsPString left, TpsPString right) => !(left == right);
}
