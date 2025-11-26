using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace TpsParser.Data;

/// <summary>
/// A strongly-typed connection string builder for <see cref="TpsDbConnection"/> instances.
/// </summary>
public sealed class TpsConnectionStringBuilder : System.Data.Common.DbConnectionStringBuilder
{
    /// <summary>
    /// Instantiates a new connection string builder.
    /// </summary>
    public TpsConnectionStringBuilder()
    { }

    /// <summary>
    /// Instantiates a new connection string builder from an existing connection string.
    /// </summary>
    /// <param name="ConnectionString"></param>
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

    private bool TryGetEncodingValue(string key, [NotNullWhen(true)] out Encoding? value)
    {
        if (!TryGetValue(key, out object? maybeObject))
        {
            value = null;
            return false;
        }

        if (maybeObject is Encoding encoding)
        {
            value = encoding;
            return true;
        }

        if (maybeObject is not string stringValue)
        {
            value = null;
            return false;
        }

        // For all ANSI, OEM, and exotic encodings including Windows-1252
        Encoding? maybeCodePagesEncoding = CodePagesEncodingProvider.Instance.GetEncoding(stringValue);

        if (maybeCodePagesEncoding is not null)
        {
            value = maybeCodePagesEncoding;
            return true;
        }

        // For UTF-8, US-ASCII, ISO-8859-1, etc.
        try
        {
            value = Encoding.GetEncoding(stringValue);
            return true;
        }
        catch (ArgumentException)
        {
            value = null;
            return false;
        }
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
    public Encoding? ContentEncoding
    {
        get => TryGetEncodingValue(ContentEncodingName, out Encoding? value)
            ? value
            : null;
        set => this[ContentEncodingName] = value?.WebName;
    }
    internal const string ContentEncodingName = "ContentEncoding";

    /// <inheritdoc cref="EncodingOptions.MetadataEncoding"/>
    public Encoding? MetadataEncoding
    {
        get => TryGetEncodingValue(MetadataEncodingName, out Encoding? value)
            ? value
            : null;
        set => this[MetadataEncodingName] = value?.WebName;
    }
    internal const string MetadataEncodingName = "MetadataEncoding";

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
    [AllowNull]
    public override object this[string keyword]
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
                    if (!Enum.TryParse<ErrorHandling>(stringValue, ignoreCase: true, out var parsed))
                    {
                        throw new ArgumentException($"{stringValue} is not a valid {nameof(Data.ErrorHandling)}.", nameof(value));
                    }
                }
            }
            else if (string.Equals(keyword, ErrorHandlingThrowOnRleDecompressionErrorName, comparer))
            {
                if (value is string stringValue)
                {
                    if (!bool.TryParse(stringValue, out var parsed))
                    {
                        throw new ArgumentException($"{stringValue} is not a valid bool.");
                    }
                }
            }
            else if (string.Equals(keyword, ErrorHandlingRleUndersizedDecompressionBehaviorName, comparer)
                || string.Equals(keyword, ErrorHandlingRleOversizedDecompressionBehaviorName, comparer))
            {
                if (value is string stringValue)
                {
                    if (!Enum.TryParse<RleSizeMismatchBehavior>(stringValue, ignoreCase: true, out var parsed))
                    {
                        throw new ArgumentException($"{stringValue} is not a valid {nameof(RleSizeMismatchBehavior)}.", nameof(value));
                    }
                }
            }
            else if (string.Equals(keyword, ContentEncodingName, comparer)
                || string.Equals(keyword, MetadataEncodingName, comparer))
            {
                if (value is string stringValue)
                {
                    bool hasEncoding =
                        Encoding.GetEncodings().Select(e => e.Name).Any(name => string.Equals(stringValue, name, comparer))
                        || CodePagesEncodingProvider.Instance.GetEncodings().Select(e => e.Name).Any(name => string.Equals(stringValue, name, comparer));

                    if (!hasEncoding)
                    {
                        throw new ArgumentException($"{stringValue} is not recognized as a valid encoding.");
                    }
                }
                else if (value is Encoding encoding)
                {
                    base[keyword] = encoding.WebName;
                    return;
                }
            }
            
            base[keyword] = value; 
        }
    }

    /// <summary>
    /// Gets a <see cref="EncodingOptions"/> instance based on the values set in the connection string.
    /// </summary>
    /// <returns></returns>
    public EncodingOptions GetEncodingOptions()
    {
        var def = EncodingOptions.Default;

        return def with
        {
            ContentEncoding = ContentEncoding ?? def.ContentEncoding,
            MetadataEncoding = MetadataEncoding ?? def.MetadataEncoding,
        };
    }

    /// <summary>
    /// Gets a <see cref="ErrorHandlingOptions"/> instance based on the values set in the connection string.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ErrorHandlingOptions GetErrorHandlingOptions()
    {
        static TpsParser.RleSizeMismatchBehavior? Map(RleSizeMismatchBehavior? b) => b switch
        {
            RleSizeMismatchBehavior.Throw => TpsParser.RleSizeMismatchBehavior.Throw,
            RleSizeMismatchBehavior.Skip => TpsParser.RleSizeMismatchBehavior.Skip,
            RleSizeMismatchBehavior.Allow => TpsParser.RleSizeMismatchBehavior.Allow,
            null => null,
            _ => throw new NotImplementedException()
        };

        ErrorHandlingOptions def = ErrorHandling switch
        {
            null or Data.ErrorHandling.Default => ErrorHandlingOptions.Default,
            Data.ErrorHandling.Strict => ErrorHandlingOptions.Strict,
            _ => throw new NotImplementedException()
        };

        return def with
        {
            ThrowOnRleDecompressionError = ErrorHandlingThrowOnRleDecompressionError ?? def.ThrowOnRleDecompressionError,
            RleUndersizedDecompressionBehavior = Map(ErrorHandlingRleUndersizedDecompressionBehavior) ?? def.RleUndersizedDecompressionBehavior,
            RleOversizedDecompressionBehavior = Map(ErrorHandlingRleOversizedDecompressionBehavior) ?? def.RleOversizedDecompressionBehavior
        };
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
