using NUnit.Framework;
using System.Collections.Generic;

namespace TpsParser.Data.Tests;

internal sealed class TestQueryParser
{
    [TestCaseSource(typeof(ShouldParseFileNameAndTableName_TestCases), nameof(ShouldParseFileNameAndTableName_TestCases.Cases))]
    public FileTableName ShouldParseFileNameAndTableName(string commandText)
    {
        return QueryParser.GetFileTableName(commandText);
    }

    private static class ShouldParseFileNameAndTableName_TestCases
    {
        public static IEnumerable<TestCaseData> Cases()
        {
            yield return new TestCaseData<string>(
                """
                select * from f
                """)
                .Returns(new FileTableName("f", null));

            yield return new TestCaseData<string>(
                """
                select * from f.x
                """)
                .Returns(new FileTableName("f.x", null));

            yield return new TestCaseData<string>(
                """
                select * from .x
                """)
                .Returns(new FileTableName(".x", null));

            yield return new TestCaseData<string>(
                """
                select * from ff
                """)
                .Returns(new FileTableName("ff", null));



            yield return new TestCaseData<string>(
                """
                select * from f\!t
                """)
                .Returns(new FileTableName("f", "t"));

            yield return new TestCaseData<string>(
                """
                select * from f.x\!t
                """)
                .Returns(new FileTableName("f.x", "t"));

            yield return new TestCaseData<string>(
                """
                select * from .x\!t
                """)
                .Returns(new FileTableName(".x", "t"));

            yield return new TestCaseData<string>(
                """
                select * from ff\!t
                """)
                .Returns(new FileTableName("ff", "t"));


            yield return new TestCaseData<string>(
                """
                select * from "f"
                """)
                .Returns(new FileTableName("f", null));

            yield return new TestCaseData<string>(
                """
                select * from "f.x"
                """)
                .Returns(new FileTableName("f.x", null));

            yield return new TestCaseData<string>(
                """
                select * from ".x"
                """)
                .Returns(new FileTableName(".x", null));

            yield return new TestCaseData<string>(
                """
                select * from "ff"
                """)
                .Returns(new FileTableName("ff", null));



            yield return new TestCaseData<string>(
                """
                select * from "f\!t"
                """)
                .Returns(new FileTableName("f", "t"));

            yield return new TestCaseData<string>(
                """
                select * from "f.x\!t"
                """)
                .Returns(new FileTableName("f.x", "t"));

            yield return new TestCaseData<string>(
                """
                select * from ".x\!t"
                """)
                .Returns(new FileTableName(".x", "t"));

            yield return new TestCaseData<string>(
                """
                select * from "ff\!t"
                """)
                .Returns(new FileTableName("ff", "t"));



            yield return new TestCaseData<string>(
                """
                select * from [f]
                """)
                .Returns(new FileTableName("f", null));

            yield return new TestCaseData<string>(
                """
                select * from [f.x]
                """)
                .Returns(new FileTableName("f.x", null));

            yield return new TestCaseData<string>(
                """
                select * from [.x]
                """)
                .Returns(new FileTableName(".x", null));

            yield return new TestCaseData<string>(
                """
                select * from [ff]
                """)
                .Returns(new FileTableName("ff", null));



            yield return new TestCaseData<string>(
                """
                select * from [f\!t]
                """)
                .Returns(new FileTableName("f", "t"));

            yield return new TestCaseData<string>(
                """
                select * from [f.x\!t]
                """)
                .Returns(new FileTableName("f.x", "t"));

            yield return new TestCaseData<string>(
                """
                select * from [.x\!t]
                """)
                .Returns(new FileTableName(".x", "t"));

            yield return new TestCaseData<string>(
                """
                select * from [ff\!t]
                """)
                .Returns(new FileTableName("ff", "t"));
        }
    }
}
