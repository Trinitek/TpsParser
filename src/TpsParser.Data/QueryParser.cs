using System;
using System.Text.RegularExpressions;

namespace TpsParser.Data;

internal sealed record FileTableName(string FileName, string? TableName);

internal static partial class QueryParser
{
    public static FileTableName GetFileTableName(string commandText)
    {
        var regex = GetFileTableNameRegex();
        
        var match = regex.Match(commandText);
        
        if (!match.Success)
        {
            throw new ArgumentException(@"The command text must be in the format 'SELECT * FROM ""<FileName>[\!<TableName>]""'.", nameof(commandText));
        }

        string fileName = match.Groups["File"].Value;
        string tableName = match.Groups["TableName"].Value;

        return new (
            FileName: fileName,
            TableName: string.IsNullOrEmpty(tableName) ? null : tableName);
    }

    private const RegexOptions Options = RegexOptions.None
        | RegexOptions.ExplicitCapture
        | RegexOptions.IgnoreCase
        | RegexOptions.IgnorePatternWhitespace
        | RegexOptions.Compiled;

    // language=regex
    private const string FileTableNameRegex =
        """
        \s*SELECT\s+\*\s+FROM\s+
        (
            (   (?<File>([\w-_]*\.?[\w-_]+)) (\\!(?<TableName>\w+))?   )
            |
            (  "(?<File>([\w-_]*\.?[\w-_]+)) (\\!(?<TableName>\w+))? " )
            |
            ( \[(?<File>([\w-_]*\.?[\w-_]+)) (\\!(?<TableName>\w+))?\] )
        )
        \s*(;)?\s*
        """;


    [GeneratedRegex(FileTableNameRegex, Options)]
    private static partial Regex GetFileTableNameRegex();
}