using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace TpsParser.Data;

/// <summary>
/// Represents a database connection over a folder containing one or more TPS files.
/// </summary>
public class TpsDbConnection : DbConnection
{
    /// <summary>
    /// Gets or sets a string used to open the connection.
    /// </summary>
    [AllowNull]
    public override string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets the data source. Always an empty string.
    /// </summary>
    public override string DataSource { get; } = string.Empty;

    /// <summary>
    /// Gets the server version. Always an empty string.
    /// </summary>
    public override string ServerVersion { get; } = string.Empty;

    /// <inheritdoc/>
    public override ConnectionState State => _state;
    private ConnectionState _state;

    /// <summary>
    /// Gets the name of the current database. This is a path to a folder that contains one or more TPS files.
    /// </summary>
    public override string Database => _database;
    private string _database = string.Empty;

    /// <summary>
    /// Gets the current context containing the <see cref="TpsFile"/> and other info used by this connection,
    /// or <see langword="null"/> if <see cref="State"/> is not <see cref="ConnectionState.Open"/>.
    /// </summary>
    public TpsFileConnectionContext? CurrentFileContext => _currentFileContext;
    private TpsFileConnectionContext? _currentFileContext = null;

    /// <summary>
    /// Instantiates a new <see cref="TpsDbConnection"/>.
    /// </summary>
    public TpsDbConnection()
    { }

    /// <summary>
    /// Instantiates a new <see cref="TpsDbConnection"/>.
    /// </summary>
    /// <param name="connectionString"></param>
    public TpsDbConnection(string? connectionString)
    {
        ConnectionString = connectionString;
    }

    private void AssertIsOpen()
    {
        if (State != ConnectionState.Open)
        {
            throw new InvalidOperationException("Connection is not open.");
        }
    }

    /// <inheritdoc/>
    public new IDbTransaction BeginTransaction()
    {
        throw new NotSupportedException("Transactions are not supported.");
    }

    /// <inheritdoc/>
    public new IDbTransaction BeginTransaction(IsolationLevel il)
    {
        throw new NotSupportedException("Transactions are not supported.");
    }
    
    /// <summary>
    /// Changes the current database. The database name is the folder path that contains the TPS file.
    /// </summary>
    /// <param name="databaseName"></param>
    /// <exception cref="DirectoryNotFoundException"></exception>
    public override void ChangeDatabase(string? databaseName)
    {
        // Do nothing if the folder path is exactly the same.
        // Take care not to do case-insensitive matching; we might be running on a case-sensitive filesystem.
        if (_database == databaseName)
        {
            return;
        }

        if (!Directory.Exists(databaseName))
        {
            throw new DirectoryNotFoundException($"Cannot switch database folder to new path because it doesn't exist: {databaseName}");
        }

        _database = databaseName;
        _currentFileContext = null;
    }

    /// <inheritdoc/>
    public override void Close()
    {
        _currentFileContext = null;
        _state = ConnectionState.Closed;
    }

    /// <inheritdoc/>
    public override void Open()
    {
        var csBuilder = new TpsConnectionStringBuilder()
        {
            ConnectionString = ConnectionString,
        };

        ChangeDatabase(csBuilder.Folder);

        _state = ConnectionState.Open;
    }

    /// <inheritdoc/>
    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        throw new NotSupportedException("Transactions are not supported.");
    }

    /// <inheritdoc/>
    protected override DbCommand CreateDbCommand()
    {
        return new TpsDbCommand()
        {
            Connection = this,
        };
    }

    /// <summary>
    /// Gets the <see cref="TpsFile"/> and associated information for this connection.
    /// </summary>
    /// <param name="requestedFileName"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    internal TpsFileConnectionContext GetOrOpenTpsFile(string requestedFileName)
    {
        AssertIsOpen();

        string[] filenames =
        [
            $@"{requestedFileName}.tps",
            $@"{requestedFileName}",
        ];

        string folder = Database;

        string? foundFileName = null;
        string? foundFilePath = null;

        if (string.IsNullOrWhiteSpace(folder))
        {
            throw new InvalidOperationException("Folder was not specified in connection string.");
        }

        foreach (var filename in filenames)
        {
            var Path = System.IO.Path.Combine(folder, filename);

            if (File.Exists(Path))
            {
                foundFileName = filename;
                foundFilePath = Path;
                break;
            }
        }

        if (foundFilePath is null)
        {
            throw new FileNotFoundException($@"Unable to locate database file '{requestedFileName}'.");
        }

        if (_currentFileContext is null || _currentFileContext.FileName != foundFileName)
        {
            var csBuilder = new TpsConnectionStringBuilder(ConnectionString);

            var encodingOptions = csBuilder.GetEncodingOptions();
            var errorOptions = csBuilder.GetErrorHandlingOptions();

            using var fs = new FileStream(foundFilePath, FileMode.Open, FileAccess.Read);

            var tpsFile = new TpsFile(
                fs,
                encodingOptions: encodingOptions,
                errorHandlingOptions: errorOptions);

            _currentFileContext = new(
                TpsFile: tpsFile,
                FileName: foundFileName!,
                MemoIndexer: new(tpsFile));

            return _currentFileContext;
        }

        return _currentFileContext;
    }
}

/// <summary>
/// Represents file information used by an open <see cref="TpsDbConnection"/>.
/// </summary>
/// <param name="TpsFile"></param>
/// <param name="FileName"></param>
/// <param name="MemoIndexer"></param>
public sealed record TpsFileConnectionContext(
    TpsFile TpsFile,
    string FileName,
    MemoIndexer MemoIndexer);