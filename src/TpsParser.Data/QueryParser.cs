using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace TpsParser.Data;

public static partial class QueryParser
{
    public static string GetFileName(string commandText)
    {
        var regex = GetFileNameRegex();
        var match = regex.Match(commandText);
        
        if (!match.Success)
        {
            throw new ArgumentException($"The command text must be in the format 'SELECT * FROM <FILENAME>'.", nameof(commandText));
        }
        
        var ret = new[]{
            match.Groups["Value1"].Value,
            match.Groups["Value2"].Value,
            match.Groups["Value3"].Value,
        }.Where(x => !string.IsNullOrEmpty(x)).FirstOrDefault() ?? string.Empty;

        return ret;
    }

    private const RegexOptions Options = RegexOptions.None
        | RegexOptions.ExplicitCapture
        | RegexOptions.IgnoreCase
        | RegexOptions.IgnorePatternWhitespace
        | RegexOptions.Compiled
        ;

    const string SPACE = $@"(\s|\r|\n)";
    const string REGEX = $@"
        {SPACE}*
        SELECT
        {SPACE}*
        \*
        {SPACE}*
        FROM
        (
            ({SPACE}+ (?<Value1> (\w|\.)+))
                |
            ({SPACE}* \""(?<Value2>.*)\"")
                |
            ({SPACE}* \[(?<Value3>.*)\])
        )

        {SPACE}*
        (;)?
        {SPACE}*
";


    [GeneratedRegex(REGEX, Options)]
    private static partial Regex GetFileNameRegex();
}