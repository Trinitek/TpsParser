using System;
using System.Data;
using System.Data.Common;
using System.IO;

namespace TpsParser.Data;

public class TpsDbConnection : DbConnection {
    
    public override string ConnectionString { get; set; }
    
    public override string DataSource { get; }
    public override string ServerVersion { get; }


    public override ConnectionState State => _state;
    private ConnectionState _state;

    public override string Database => _database;
    private string _database = string.Empty;

    public new IDbTransaction BeginTransaction()
    {
        throw new NotImplementedException();
    }

    public new IDbTransaction BeginTransaction(IsolationLevel il)
    {
        throw new NotImplementedException();
    }


    public override void ChangeDatabase(string? databaseName)
    {
        if (Directory.Exists(databaseName))
        {
            _database = databaseName;
        }
        else
        {
            throw new DirectoryNotFoundException(databaseName);
        }
    }

    public override void Close()
    {
        _state = ConnectionState.Closed;
    }

    public override void Open()
    {
        var csBuilder = new TpsConnectionStringBuilder()
        {
            ConnectionString = ConnectionString,
        };

        ChangeDatabase(csBuilder.Folder);

        _state = ConnectionState.Open;
    }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        throw new NotImplementedException();
    }

    protected override DbCommand CreateDbCommand()
    {
        var ret = new TpsDbCommand()
        {
            Connection = this,
        };

        return ret;
    }
}