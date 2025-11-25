using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace TpsParser.Data;

public sealed class TpsConnectionStringBuilder : System.Data.Common.DbConnectionStringBuilder
{

    public TpsConnectionStringBuilder()
    { }

    public TpsConnectionStringBuilder(string? ConnectionString)
    {
        this.ConnectionString = ConnectionString;
    }

    private bool TryGetEnumValue<T>(string key, [NotNullWhen(true)] out T? value) where T : struct, Enum
    {
        if (!TryGetValue(key, out object? maybeObject))
        {
            value = null;
            return false;
        }

        if (maybeObject is T tValue)
        {
            value = tValue;
            return true;
        }

        if (maybeObject is not string sValue)
        {
            value = null;
            return false;
        }

        if (Enum.TryParse(sValue, out T parsedTValue))
        {
            value = parsedTValue;
            return true;
        }

        value = null;
        return false;
    }

    /// <summary>
    /// Gets or sets the base error handling options to use when reading the file.
    /// </summary>
    public ErrorHandling? ErrorHandling
    {
        get => TryGetEnumValue(ErrorHandlingName, out ErrorHandling? value)
            ? value
            : null;
        set => this[ErrorHandlingName] = value;
    }
    internal const string ErrorHandlingName = "ErrorHandling";

    /// <inheritdoc cref="ErrorHandlingOptions.ThrowOnRleDecompressionError"/>
    public bool? ErrorHandlingThrowOnRleDecompressionError
    {
        get
        {
            if (!TryGetValue(ErrorHandlingThrowOnRleDecompressionErrorName, out object? maybeValue))
            {
                return null;
            }
            if (maybeValue is bool boolValue)
            {
                return boolValue;
            }
            if (bool.TryParse(maybeValue as string, out boolValue))
            {
                return boolValue;
            }
            
            return null;
        }
        set => this[ErrorHandlingThrowOnRleDecompressionErrorName] = value;
    }
    internal const string ErrorHandlingThrowOnRleDecompressionErrorName = "ErrorHandling.ThrowOnRleDecompressionError";

    /// <inheritdoc cref="ErrorHandlingOptions.RleUndersizedDecompressionBehavior"/>
    public RleSizeMismatchBehavior? ErrorHandlingRleUndersizedDecompressionBehavior
    {
        get => TryGetEnumValue(ErrorHandlingRleUndersizedDecompressionBehaviorName, out RleSizeMismatchBehavior? value)
            ? value
            : null;
        set => this[ErrorHandlingRleUndersizedDecompressionBehaviorName] = value;
    }
    internal const string ErrorHandlingRleUndersizedDecompressionBehaviorName = "ErrorHandling.RleUndersizedDecompressionBehavior";

    /// <inheritdoc cref="ErrorHandlingOptions.RleOversizedDecompressionBehavior"/>
    public RleSizeMismatchBehavior? ErrorHandlingRleOversizedDecompressionBehavior
    {
        get => TryGetEnumValue(ErrorHandlingRleOversizedDecompressionBehaviorName, out RleSizeMismatchBehavior? value)
            ? value
            : null;
        set => this[ErrorHandlingRleOversizedDecompressionBehaviorName] = value;
    }
    internal const string ErrorHandlingRleOversizedDecompressionBehaviorName = "ErrorHandling.RleOversizedDecompressionBehavior";

    /// <inheritdoc cref="EncodingOptions.ContentEncoding"/>
    public Encoding ContentEncoding
    {
        get => TryGetValue(ContentEncodingName, out object? maybeValue) && maybeValue is Encoding value
            ? value
            : EncodingOptions.Default.ContentEncoding;
        set => this[ContentEncodingName] = value;
    }
    internal const string ContentEncodingName = "ContentEncoding";

    /// <inheritdoc cref="EncodingOptions.MetadataEncoding"/>
    public Encoding MetadataEncoding
    {
        get => TryGetValue(MetadataEncodingName, out object? maybeValue) && maybeValue is Encoding value
            ? value
            : EncodingOptions.Default.MetadataEncoding;
        set => this[ContentEncodingName] = value;
    }
    internal const string MetadataEncodingName = "ContentEncoding";

    /// <summary>
    /// Gets or sets the folder from which to read. The folder should contain one or more TPS files.
    /// </summary>
    public string? Folder
    {
        get => TryGetValue(FolderName, out object? maybeValue) && maybeValue is string value
            ? value
            : null;
        set => this[nameof(Folder)] = value;
    }
    internal const string FolderName = "Folder";

    /// <inheritdoc/>
    public override object? this[string keyword]
    {
        get
        {
            return base[keyword];
        }
        set
        {
            var comparer = StringComparison.InvariantCultureIgnoreCase;

            if (string.Equals(keyword, ErrorHandlingName, comparer))
            {
                if (value is string stringValue)
                {
                    if (Enum.TryParse<ErrorHandling>(stringValue, ignoreCase: true, out var parsed))
                    {
                        base[keyword] = parsed;
                    }
                    else
                    {
                        throw new ArgumentException($"{stringValue} is not a valid {nameof(Data.ErrorHandling)}.", nameof(value));
                    }
                }
            }
            else if (string.Equals(keyword, ErrorHandlingRleUndersizedDecompressionBehaviorName, comparer)
                || string.Equals(keyword, ErrorHandlingRleOversizedDecompressionBehaviorName, comparer))
            {
                if (value is string stringValue)
                {
                    if (Enum.TryParse<RleSizeMismatchBehavior>(stringValue, ignoreCase: true, out var parsed))
                    {
                        base[keyword] = parsed;
                    }
                    else
                    {
                        throw new ArgumentException($"{stringValue} is not a valid {nameof(Data.ErrorHandling)}.", nameof(value));
                    }
                }
            }

            base[keyword] = value;
        }
    }
}

/// <summary>
/// Represents one of the base <see cref="ErrorHandlingOptions"/> values.
/// </summary>
public enum ErrorHandling
{
    /// <summary>
    /// Represents the equivalent value for <see cref="ErrorHandlingOptions.Default"/>.
    /// </summary>
    Default,

    /// <summary>
    /// Represents the equivalent value for <see cref="ErrorHandlingOptions.Strict"/>.
    /// </summary>
    Strict
}

/// <summary>
/// Represents an equivalent <see cref="TpsParser.RleSizeMismatchBehavior"/>.
/// </summary>
public enum RleSizeMismatchBehavior
{
    /// <summary>
    /// Represents the equivalent value for <see cref="TpsParser.RleSizeMismatchBehavior.Throw"/>.
    /// </summary>
    Throw,

    /// <summary>
    /// Represents the equivalent value for <see cref="TpsParser.RleSizeMismatchBehavior.Allow"/>.
    /// </summary>
    Allow,

    /// <summary>
    /// Represents the equivalent value for <see cref="TpsParser.RleSizeMismatchBehavior.Skip"/>.
    /// </summary>
    Skip
}
