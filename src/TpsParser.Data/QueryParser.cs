using System;
using System.Text.RegularExpressions;

namespace TpsParser.Data;

internal sealed record FileTableName(string FileName, string? TableName);

internal static partial class QueryParser
{
    public static FileTableName GetFileTableName(string commandText)
    {
        var regex = GetFileNameRegex();
        
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
    private const string REGEX =
        """
        \s*SELECT\s+\*\s+FROM\s+
        (
            (   (?<File>(\w*\.?\w+)) (\\!(?<TableName>\w+))?   )
            |
            ( ""(?<File>(\w*\.?\w+)) (\\!(?<TableName>\w+))?"" )
            |
            ( \[(?<File>(\w*\.?\w+)) (\\!(?<TableName>\w+))?\] )
        )
        \s*(;)?\s*
        """;


    [GeneratedRegex(REGEX, Options)]
    private static partial Regex GetFileNameRegex();
}