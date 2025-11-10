using System;
using System.Data;
using System.Data.Common;
using System.IO;

namespace TpsParser.Data;

public partial class TpsDbCommand : DbCommand {
    public override string CommandText { get; set; }
    public override int CommandTimeout { get; set; }
    public override CommandType CommandType { get; set; }
    public override bool DesignTimeVisible { get; set; }
    public override UpdateRowSource UpdatedRowSource { get; set; }
    protected override DbConnection? DbConnection { get; set; }
    protected override DbParameterCollection DbParameterCollection { get; }
    protected override DbTransaction? DbTransaction { get; set; }

    public override void Cancel() {
        
    }

    public override int ExecuteNonQuery()
    {
        throw new NotImplementedException();
    }

    public override object? ExecuteScalar()
    {
        throw new NotImplementedException();
    }

    public override void Prepare()
    {
        // Do nothing
    }

    protected override DbParameter CreateDbParameter()
    {
        throw new NotImplementedException();
    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        var BaseFileName = QueryParser.GetFileName(CommandText);

        var FileNames = new[] {
            $@"{BaseFileName}.tps",
            $@"{BaseFileName}",
        };

        var Folder = (Connection as TpsDbConnection)?.Database;
        var FoundFile = default(string?);

        if(Folder is { }) {
            foreach(var Filename in FileNames) {
                var Path = System.IO.Path.Combine(Folder, Filename);
                if (System.IO.File.Exists(Path)) {
                    FoundFile = Path;
                    break;
                }
            }
        }

        if (FoundFile is null) {
            throw new FileNotFoundException($@"Unable to find database file.");
        }

        var Parser = new TpsParser(FoundFile);

        var Definitions = Parser.TpsFile.GetTableDefinitions(true);

        var ret = new TpsDataReader(Parser, Definitions);
        ret.NextResult();
        return ret;
        
    }

}