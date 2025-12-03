using NUnit.Framework;
using System.Collections.Generic;

namespace TpsParser.Data.Tests;

internal sealed class TestQueryParser
{
    [TestCaseSource(typeof(ShouldParseFileNameAndTableName_TestCases), nameof(ShouldParseFileNameAndTableName_TestCases.Cases))]
    [TestCaseSource(typeof(ShouldParseFileNameAndTableName_Dashes_TestCases), nameof(ShouldParseFileNameAndTableName_Dashes_TestCases.Cases))]
    [TestCaseSource(typeof(ShouldParseFileNameAndTableName_Underscores_TestCases), nameof(ShouldParseFileNameAndTableName_Underscores_TestCases.Cases))]
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

    private static class ShouldParseFileNameAndTableName_Dashes_TestCases
    {
        public static IEnumerable<TestCaseData> Cases()
        {
            yield return new TestCaseData<string>(
                """
                select * from -f
                """)
                .Returns(new FileTableName("-f", null));

            yield return new TestCaseData<string>(
                """
                select * from f-
                """)
                .Returns(new FileTableName("f-", null));

            yield return new TestCaseData<string>(
                """
                select * from f-f
                """)
                .Returns(new FileTableName("f-f", null));



            yield return new TestCaseData<string>(
                """
                select * from -f.x
                """)
                .Returns(new FileTableName("-f.x", null));

            yield return new TestCaseData<string>(
                """
                select * from f-.x
                """)
                .Returns(new FileTableName("f-.x", null));

            yield return new TestCaseData<string>(
                """
                select * from f-f.x
                """)
                .Returns(new FileTableName("f-f.x", null));



            yield return new TestCaseData<string>(
                """
                select * from .-x
                """)
                .Returns(new FileTableName(".-x", null));

            yield return new TestCaseData<string>(
                """
                select * from .x-
                """)
                .Returns(new FileTableName(".x-", null));

            yield return new TestCaseData<string>(
                """
                select * from .x-x
                """)
                .Returns(new FileTableName(".x-x", null));
        }
    }

    private static class ShouldParseFileNameAndTableName_Underscores_TestCases
    {
        public static IEnumerable<TestCaseData> Cases()
        {
            yield return new TestCaseData<string>(
                """
                select * from _f
                """)
                .Returns(new FileTableName("_f", null));

            yield return new TestCaseData<string>(
                """
                select * from f_
                """)
                .Returns(new FileTableName("f_", null));

            yield return new TestCaseData<string>(
                """
                select * from f_f
                """)
                .Returns(new FileTableName("f_f", null));



            yield return new TestCaseData<string>(
                """
                select * from _f.x
                """)
                .Returns(new FileTableName("_f.x", null));

            yield return new TestCaseData<string>(
                """
                select * from f_.x
                """)
                .Returns(new FileTableName("f_.x", null));

            yield return new TestCaseData<string>(
                """
                select * from f_f.x
                """)
                .Returns(new FileTableName("f_f.x", null));



            yield return new TestCaseData<string>(
                """
                select * from ._x
                """)
                .Returns(new FileTableName("._x", null));

            yield return new TestCaseData<string>(
                """
                select * from .x_
                """)
                .Returns(new FileTableName(".x_", null));

            yield return new TestCaseData<string>(
                """
                select * from .x_x
                """)
                .Returns(new FileTableName(".x_x", null));
        }
    }
}
