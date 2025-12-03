using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace TpsParser.Data;

// Referencing Microsoft.Data.Sqlite SqliteConnectionStringBuilder implementation
// https://github.com/dotnet/efcore/blob/9a868352299586a7e23b736d945f2931b1e822bf/src/Microsoft.Data.Sqlite.Core/SqliteConnectionStringBuilder.cs

/// <summary>
/// A strongly-typed connection string builder for <see cref="TpsDbConnection"/> instances.
/// </summary>
public class TpsConnectionStringBuilder : System.Data.Common.DbConnectionStringBuilder
{
    private const string DataSourceKeyword = "Data Source";
    private const string DataSourceNoSpaceKeyword = "DataSource";
    private const string FolderKeyword = "Folder";
    private const string FlattenCompoundStructureResultsKeyword = "FlattenCompoundStructureResults";
    private const string ContentEncodingKeyword = "ContentEncoding";
    private const string MetadataEncodingKeyword = "MetadataEncoding";
    private const string ErrorHandlingKeyword = "ErrorHandling";
    private const string ErrorHandlingThrowOnRleDecompressionErrorKeyword = "ErrorHandling.ThrowOnRleDecompressionError";
    private const string ErrorHandlingRleOversizedDecompressionBehaviorKeyword = "ErrorHandling.RleOversizedDecompressionBehavior";
    private const string ErrorHandlingRleUndersizedDecompressionBehaviorKeyword = "ErrorHandling.RleUndersizedDecompressionBehavior";

    private enum Keywords
    {
        DataSource,
        FlattenCompoundStructureResults,
        ContentEncoding,
        MetadataEncoding,
        ErrorHandling,
        ErrorHandlingThrowOnRleDecompressionError,
        ErrorHandlingRleOversizedDecompressionBehavior,
        ErrorHandlingRleUndersizedDecompressionBehavior,
    }

    private static readonly ReadOnlyCollection<string> _validKeywords;
    private static readonly ReadOnlyDictionary<string, Keywords> _keywords;

    private string? _dataSource = null;
    private bool? _flattenCompoundStructureResults = null;
    private string? _contentEncoding = null;
    private string? _metadataEncoding = null;
    private ErrorHandling? _errorHandling = null;
    private bool? _errorHandlingThrowOnRleDecompressionError = null;
    private RleSizeMismatchBehavior? _errorHandlingRleOversizedDecompressionBehavior = null;
    private RleSizeMismatchBehavior? _errorHandlingRleUndersizedDecompressionBehavior = null;

    static TpsConnectionStringBuilder()
    {
        var validKeywords = new string[8];
        validKeywords[(int)Keywords.DataSource] = DataSourceKeyword;
        validKeywords[(int)Keywords.FlattenCompoundStructureResults] = FlattenCompoundStructureResultsKeyword;
        validKeywords[(int)Keywords.ContentEncoding] = ContentEncodingKeyword;
        validKeywords[(int)Keywords.MetadataEncoding] = MetadataEncodingKeyword;
        validKeywords[(int)Keywords.ErrorHandling] = ErrorHandlingKeyword;
        validKeywords[(int)Keywords.ErrorHandlingThrowOnRleDecompressionError] = ErrorHandlingThrowOnRleDecompressionErrorKeyword;
        validKeywords[(int)Keywords.ErrorHandlingRleOversizedDecompressionBehavior] = ErrorHandlingRleOversizedDecompressionBehaviorKeyword;
        validKeywords[(int)Keywords.ErrorHandlingRleUndersizedDecompressionBehavior] = ErrorHandlingRleUndersizedDecompressionBehaviorKeyword;
        _validKeywords = validKeywords.AsReadOnly();

        _keywords = new Dictionary<string, Keywords>(10, StringComparer.OrdinalIgnoreCase)
        {
            [DataSourceKeyword] = Keywords.DataSource,
            [FlattenCompoundStructureResultsKeyword] = Keywords.FlattenCompoundStructureResults,
            [ContentEncodingKeyword] = Keywords.ContentEncoding,
            [MetadataEncodingKeyword] = Keywords.MetadataEncoding,
            [ErrorHandlingKeyword] = Keywords.ErrorHandling,
            [ErrorHandlingThrowOnRleDecompressionErrorKeyword] = Keywords.ErrorHandlingThrowOnRleDecompressionError,
            [ErrorHandlingRleOversizedDecompressionBehaviorKeyword] = Keywords.ErrorHandlingRleOversizedDecompressionBehavior,
            [ErrorHandlingRleUndersizedDecompressionBehaviorKeyword] = Keywords.ErrorHandlingRleUndersizedDecompressionBehavior,

            // Aliases
            [DataSourceNoSpaceKeyword] = Keywords.DataSource,
            [FolderKeyword] = Keywords.DataSource,
        }.AsReadOnly();
    }

    /// <summary>
    /// Instantiates a new connection string builder.
    /// </summary>
    public TpsConnectionStringBuilder()
    { }

    /// <summary>
    /// Instantiates a new connection string builder from an existing connection string.
    /// </summary>
    /// <param name="connectionString"></param>
    public TpsConnectionStringBuilder(string? connectionString)
    {
        ConnectionString = connectionString;
    }

    private object? GetAt(Keywords index) => index switch
    {
        Keywords.DataSource => DataSource,
        Keywords.FlattenCompoundStructureResults => FlattenCompoundStructureResults,
        Keywords.ContentEncoding => ContentEncoding,
        Keywords.MetadataEncoding => MetadataEncoding,
        Keywords.ErrorHandling => ErrorHandling,
        Keywords.ErrorHandlingThrowOnRleDecompressionError => ErrorHandlingThrowOnRleDecompressionError,
        Keywords.ErrorHandlingRleOversizedDecompressionBehavior => ErrorHandlingRleOversizedDecompressionBehavior,
        Keywords.ErrorHandlingRleUndersizedDecompressionBehavior => ErrorHandlingRleUndersizedDecompressionBehavior,
        _ => throw new NotImplementedException($"Unexpected keyword index {index}.")
    };

    private static Keywords GetIndex(string keyword) =>
        !_keywords.TryGetValue(keyword, out var index)
        ? throw new ArgumentException($"Keyword not supported: '{keyword}'", nameof(keyword))
        : index;

    /// <summary>
    /// Gets a collection containing the keys used by the connection string.
    /// </summary>
    public override ICollection Keys => _validKeywords;

    /// <summary>
    /// Gets a collection containing the values used by the connection string.
    /// </summary>
    public override ICollection Values
    {
        get
        {
            var values = new object?[_validKeywords.Count];
            for (int i = 0; i < _validKeywords.Count; i++)
            {
                values[i] = GetAt((Keywords)i);
            }

            return new ReadOnlyCollection<object?>(values);
        }
    }

    /// <summary>
    /// Determines whether the specified key should be serialized into the connection string.
    /// </summary>
    /// <param name="keyword"></param>
    /// <returns></returns>
    public override bool ShouldSerialize(string keyword) =>
        _keywords.TryGetValue(keyword, out var index)
        && base.ShouldSerialize(_validKeywords[(int)index]);

    /// <summary>
    /// Gets the value of the specified key if it is used.
    /// </summary>
    /// <param name="keyword"></param>
    /// <param name="value"></param>
    /// <returns></returns>
#pragma warning disable CS8765 // Some properties like "ContentEncoding" may return null by design.
    public override bool TryGetValue(string keyword, out object? value)
#pragma warning restore CS8765
    {
        if (!_keywords.TryGetValue(keyword, out var index))
        {
            value = null;
            return false;
        }

        value = GetAt(index);
        return true;
    }

    private void Reset(Keywords index)
    {
        switch (index)
        {
            case Keywords.DataSource:
                _dataSource = string.Empty;
                return;
            case Keywords.FlattenCompoundStructureResults:
                _flattenCompoundStructureResults = null;
                return;
            case Keywords.ContentEncoding:
                _contentEncoding = null;
                return;
            case Keywords.MetadataEncoding:
                _metadataEncoding = null;
                return;
            case Keywords.ErrorHandling:
                _errorHandling = null;
                return;
            case Keywords.ErrorHandlingThrowOnRleDecompressionError:
                _errorHandlingThrowOnRleDecompressionError = null;
                return;
            case Keywords.ErrorHandlingRleOversizedDecompressionBehavior:
                _errorHandlingRleOversizedDecompressionBehavior = null;
                return;
            case Keywords.ErrorHandlingRleUndersizedDecompressionBehavior:
                _errorHandlingRleUndersizedDecompressionBehavior = null;
                return;
            default:
                throw new NotImplementedException($"Unexpected keyword {index}.");
        }
    }

    /// <summary>
    /// Clears the contents of the builder.
    /// </summary>
    public override void Clear()
    {
        base.Clear();

        for (int i = 0; i < _validKeywords.Count; i++)
        {
            Reset((Keywords)i);
        }
    }

    /// <summary>
    /// Determines whether the specified key is used by the connection string.
    /// </summary>
    /// <param name="keyword"></param>
    /// <returns></returns>
    public override bool ContainsKey(string keyword) =>
        _keywords.ContainsKey(keyword);

    /// <summary>
    /// Removes the specified key and its value from the connection string.
    /// </summary>
    /// <param name="keyword"></param>
    /// <returns></returns>
    public override bool Remove(string keyword)
    {
        if (!_keywords.TryGetValue(keyword, out var index)
            || !base.Remove(_validKeywords[(int)index]))
        {
            return false;
        }

        Reset(index);

        return true;
    }

    private static T ConvertToEnum<T>(object value)
        where T : struct, Enum
    {
        if (value is string stringValue)
        {
            return Enum.Parse<T>(stringValue, ignoreCase: true);
        }

        T enumValue;

        if (value is T tt)
        {
            enumValue = tt;
        }
        else if (value.GetType().IsEnum)
        {
            throw new ArgumentException($"Failed to convert '{value}' to enum of type '{typeof(T)}'.", nameof(value));
        }
        else
        {
            enumValue = (T)Enum.ToObject(typeof(T), value);
        }

        if (!Enum.IsDefined(enumValue))
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                actualValue: value,
                message: $"Invalid enum value '{enumValue}' for enum type '{typeof(T)}'.");
        }

        return enumValue;
    }

    private static string ConvertToEncodingWebName(object value)
    {
        if (value is string ss)
        {
            return ss;
        }
        else if (value is Encoding ee)
        {
            return ee.WebName;
        }

        throw new ArgumentException($"Failed to convert '{value}' to an encoding WebName.", nameof(value));
    }

    private static Encoding ConvertToEncoding(object value)
    {
        if (value is Encoding ee)
        {
            return ee;
        }
        else if (value is string encodingWebName)
        {
            // For all ANSI, OEM, and exotic encodings including Windows-1252
            Encoding? maybeCodePagesEncoding = CodePagesEncodingProvider.Instance.GetEncoding(encodingWebName);

            if (maybeCodePagesEncoding is not null)
            {
                return maybeCodePagesEncoding;
            }

            // For UTF-8, US-ASCII, ISO-8859-1, etc.
            // Will throw if no encoding is found.
            return Encoding.GetEncoding(encodingWebName);
        }

        throw new ArgumentException($"Failed to convert '{value}' to an encoding or encoding WebName.", nameof(value));
    }

    /// <summary>
    /// Gets or sets the base error handling options to use when reading the file.
    /// </summary>
    public ErrorHandling? ErrorHandling
    {
        get => _errorHandling;
        set => base[ErrorHandlingKeyword] = _errorHandling = value;
    }
    
    /// <inheritdoc cref="ErrorHandlingOptions.ThrowOnRleDecompressionError"/>
    public bool? ErrorHandlingThrowOnRleDecompressionError
    {
        get => _errorHandlingThrowOnRleDecompressionError;
        set => base[ErrorHandlingThrowOnRleDecompressionErrorKeyword] = _errorHandlingThrowOnRleDecompressionError = value;
    }
    
    /// <inheritdoc cref="ErrorHandlingOptions.RleUndersizedDecompressionBehavior"/>
    public RleSizeMismatchBehavior? ErrorHandlingRleUndersizedDecompressionBehavior
    {
        get => _errorHandlingRleUndersizedDecompressionBehavior;
        set => base[ErrorHandlingRleUndersizedDecompressionBehaviorKeyword] = _errorHandlingRleUndersizedDecompressionBehavior = value;
    }
    
    /// <inheritdoc cref="ErrorHandlingOptions.RleOversizedDecompressionBehavior"/>
    public RleSizeMismatchBehavior? ErrorHandlingRleOversizedDecompressionBehavior
    {
        get => _errorHandlingRleOversizedDecompressionBehavior;
        set => base[ErrorHandlingRleOversizedDecompressionBehaviorKeyword] = _errorHandlingRleOversizedDecompressionBehavior = value;
    }
    
    /// <inheritdoc cref="EncodingOptions.ContentEncoding"/>
    public string? ContentEncoding
    {
        get => _contentEncoding;
        set => base[ContentEncodingKeyword] = _contentEncoding = value;
    }
    
    /// <inheritdoc cref="EncodingOptions.MetadataEncoding"/>
    public string? MetadataEncoding
    {
        get => _metadataEncoding;
        set => base[MetadataEncodingKeyword] = _metadataEncoding = value;
    }

    /// <summary>
    /// Gets or sets the TPS folder path.
    /// </summary>
    [AllowNull]
    public string? DataSource
    {
        get => _dataSource;
        set => base[DataSourceKeyword] = _dataSource = value;
    }

    /// <summary>
    /// Alias of <see cref="DataSource"/>. Gets or sets the TPS folder path.
    /// </summary>
    [AllowNull]
    public string? Folder
    {
        get => DataSource;
        set => DataSource = value;
    }

    /// <summary>
    /// Gets or sets whether to flatten <see cref="TypeModel.ClaArray"/> and <see cref="TypeModel.ClaGroup"/>
    /// structures in query results.
    /// </summary>
    public bool? FlattenCompoundStructureResults
    {
        get => _flattenCompoundStructureResults;
        set => base[FlattenCompoundStructureResultsKeyword] = _flattenCompoundStructureResults = value;
    }

    /// <summary>
    /// Gets or sets the value associated with the specified key.
    /// </summary>
    /// <param name="keyword"></param>
    /// <returns></returns>
    public override object? this[string keyword]
    {
#pragma warning disable CS8764 // Some properties like "ContentEncoding" may return null by design.
        get => GetAt(GetIndex(keyword));
#pragma warning restore CS8764
        set
        {
            if (value == null)
            {
                Remove(keyword);
                return;
            }

            switch (GetIndex(keyword))
            {
                case Keywords.DataSource:
                    DataSource = Convert.ToString(value, CultureInfo.InvariantCulture);
                    return;
                case Keywords.FlattenCompoundStructureResults:
                    FlattenCompoundStructureResults = Convert.ToBoolean(value);
                    return;
                case Keywords.ContentEncoding:
                    ContentEncoding = ConvertToEncodingWebName(value);
                    return;
                case Keywords.MetadataEncoding:
                    MetadataEncoding = ConvertToEncodingWebName(value);
                    return;
                case Keywords.ErrorHandling:
                    ErrorHandling = ConvertToEnum<ErrorHandling>(value);
                    return;
                case Keywords.ErrorHandlingThrowOnRleDecompressionError:
                    ErrorHandlingThrowOnRleDecompressionError = Convert.ToBoolean(value);
                    return;
                case Keywords.ErrorHandlingRleOversizedDecompressionBehavior:
                    ErrorHandlingRleOversizedDecompressionBehavior = ConvertToEnum<RleSizeMismatchBehavior>(value);
                    return;
                case Keywords.ErrorHandlingRleUndersizedDecompressionBehavior:
                    ErrorHandlingRleUndersizedDecompressionBehavior = ConvertToEnum<RleSizeMismatchBehavior>(value);
                    return;
                default:
                    Debug.Fail($"Unexpected keyword: {keyword}");
                    return;
            }
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
            ContentEncoding = ContentEncoding is not null ? ConvertToEncoding(ContentEncoding) : def.ContentEncoding,
            MetadataEncoding = MetadataEncoding is not null ? ConvertToEncoding(MetadataEncoding) : def.MetadataEncoding,
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
