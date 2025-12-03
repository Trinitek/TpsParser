using NUnit.Framework;
using System.Data;
using System.Linq;
using TpsParser.TypeModel;

namespace TpsParser.Data.Tests.ResourceTests.ArrayOfGroups;

internal sealed class TestTpsDataReader
{
    [Test]
    public void FullRead_Unflattened()
    {
        var csBuilder = new TpsConnectionStringBuilder
        {
            Folder = "Resources"
        };

        using var connection = new TpsDbConnection(csBuilder.ConnectionString);

        connection.Open();

        using var command = new TpsDbCommand("select * from array-of-groups", connection);

        using var reader = command.ExecuteReader();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.FieldCount, Is.EqualTo(6));
            Assert.That(reader.VisibleFieldCount, Is.EqualTo(6));
            Assert.That(reader.RecordsAffected, Is.EqualTo(-1));
            Assert.That(reader.HasRows, Is.True);
            Assert.That(reader.Depth, Is.Zero);

            Assert.That(reader.GetString(0).TrimEnd(), Is.EqualTo("Hello world!"));

            var r2_ord_BArray = (ClaArray)reader.GetValue(1);

            Assert.That(r2_ord_BArray.Count, Is.EqualTo(6));
            Assert.That(r2_ord_BArray.TypeCode, Is.EqualTo(FieldTypeCode.FString));
            Assert.That(r2_ord_BArray.GetValues().Select(v => v.Value.ToString()!.TrimEnd()), Is.EqualTo(
                [
                "First",
                "Second",
                "Third",
                "Fourth",
                "Fifth",
                "Sixth"
                ]));

            var r2_ord_CArray2_3 = (ClaArray)reader.GetValue(2);

            Assert.That(r2_ord_CArray2_3.Count, Is.EqualTo(6));
            Assert.That(r2_ord_CArray2_3.TypeCode, Is.EqualTo(FieldTypeCode.Long));
            Assert.That(r2_ord_CArray2_3.GetValues().Select(v => ((ClaLong)v.Value).Value), Is.EqualTo(
                [
                1,
                2,
                3,
                4,
                5,
                6
                ]));

            var r2_ord_DGroup = (ClaGroup)reader.GetValue(3);

            Assert.That(r2_ord_DGroup.Count, Is.EqualTo(2));
            Assert.That(r2_ord_DGroup.TypeCode, Is.EqualTo(FieldTypeCode.Group));
            Assert.That(r2_ord_DGroup.GetValues().Select(v => v.Value), Is.EqualTo(
                (IClaObject[])[
                new ClaLong(123456),
                new ClaFString("Field B in Group D  "u8.ToArray())
                ]));

            var r2_ord_EGroupArray = (ClaArray)reader.GetValue(4);

            Assert.That(r2_ord_EGroupArray.Count, Is.EqualTo(2));
            Assert.That(r2_ord_EGroupArray.TypeCode, Is.EqualTo(FieldTypeCode.Group));

            {
                var r2_ord_EGroupArray_0 = (ClaGroup)r2_ord_EGroupArray.GetValue(0).Value;

                {
                    var r2_ord_EGroupArray_0_0 = (ClaArray)r2_ord_EGroupArray_0.GetValue(0).Value;

                    Assert.That(r2_ord_EGroupArray_0_0.Count, Is.EqualTo(6));
                    Assert.That(r2_ord_EGroupArray_0_0.TypeCode, Is.EqualTo(FieldTypeCode.FString));
                    Assert.That(r2_ord_EGroupArray_0_0.GetValues().Select(v => v.Value.ToString()!.TrimEnd()), Is.EqualTo(
                        [
                        "ESubA 1",
                        "ESubA 2",
                        "ESubA 3",
                        "ESubA 4",
                        "ESubA 5",
                        "ESubA 6",
                        ]));
                }

                {
                    var r2_ord_EGroupArray_0_1 = (ClaLong)r2_ord_EGroupArray_0.GetValue(1).Value;

                    Assert.That(r2_ord_EGroupArray_0_1, Is.EqualTo(new ClaLong(987654)));
                }
            }

            {
                var r2_ord_EGroupArray_1 = (ClaGroup)r2_ord_EGroupArray.GetValue(0).Value;

                {
                    var r2_ord_EGroupArray_1_0 = (ClaArray)r2_ord_EGroupArray_1.GetValue(0).Value;

                    Assert.That(r2_ord_EGroupArray_1_0.Count, Is.EqualTo(6));
                    Assert.That(r2_ord_EGroupArray_1_0.TypeCode, Is.EqualTo(FieldTypeCode.FString));
                    Assert.That(r2_ord_EGroupArray_1_0.GetValues().Select(v => v.Value.ToString()!.TrimEnd()), Is.EqualTo(
                        [
                        "ESubA 1",
                        "ESubA 2",
                        "ESubA 3",
                        "ESubA 4",
                        "ESubA 5",
                        "ESubA 6",
                        ]));
                }

                {
                    var r2_ord_EGroupArray_1_1 = (ClaLong)r2_ord_EGroupArray_1.GetValue(1).Value;

                    Assert.That(r2_ord_EGroupArray_1_1, Is.EqualTo(new ClaLong(987654)));
                }
            }

            Assert.That(reader.GetInt64(5), Is.EqualTo(2));

            // -- end

            Assert.That(reader.Read(), Is.False);
        }
    }

    [Test]
    public void FullRead_Flattened()
    {
        var csBuilder = new TpsConnectionStringBuilder
        {
            Folder = "Resources",
            FlattenCompoundStructureResults = true
        };

        using var connection = new TpsDbConnection(csBuilder.ConnectionString);

        connection.Open();

        using var command = new TpsDbCommand("select * from array-of-groups", connection);

        using var reader = command.ExecuteReader();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.FieldCount, Is.EqualTo(30));
            Assert.That(reader.VisibleFieldCount, Is.EqualTo(30));
            Assert.That(reader.RecordsAffected, Is.EqualTo(-1));
            Assert.That(reader.HasRows, Is.True);
            Assert.That(reader.Depth, Is.Zero);

            Assert.That(reader.GetString(0).TrimEnd(), Is.EqualTo("Hello world!"));
            Assert.That(reader.GetString(1).TrimEnd(), Is.EqualTo("First"));
            Assert.That(reader.GetString(2).TrimEnd(), Is.EqualTo("Second"));
            Assert.That(reader.GetString(3).TrimEnd(), Is.EqualTo("Third"));
            Assert.That(reader.GetString(4).TrimEnd(), Is.EqualTo("Fourth"));
            Assert.That(reader.GetString(5).TrimEnd(), Is.EqualTo("Fifth"));
            Assert.That(reader.GetString(6).TrimEnd(), Is.EqualTo("Sixth"));
            Assert.That(reader.GetInt32(7), Is.EqualTo(1));
            Assert.That(reader.GetInt32(8), Is.EqualTo(2));
            Assert.That(reader.GetInt32(9), Is.EqualTo(3));
            Assert.That(reader.GetInt32(10), Is.EqualTo(4));
            Assert.That(reader.GetInt32(11), Is.EqualTo(5));
            Assert.That(reader.GetInt32(12), Is.EqualTo(6));
            Assert.That(reader.GetInt32(13), Is.EqualTo(123456));
            Assert.That(reader.GetString(14).TrimEnd(), Is.EqualTo("Field B in Group D"));
            Assert.That(reader.GetString(15).TrimEnd(), Is.EqualTo("ESubA 1"));
            Assert.That(reader.GetString(16).TrimEnd(), Is.EqualTo("ESubA 2"));
            Assert.That(reader.GetString(17).TrimEnd(), Is.EqualTo("ESubA 3"));
            Assert.That(reader.GetString(18).TrimEnd(), Is.EqualTo("ESubA 4"));
            Assert.That(reader.GetString(19).TrimEnd(), Is.EqualTo("ESubA 5"));
            Assert.That(reader.GetString(20).TrimEnd(), Is.EqualTo("ESubA 6"));
            Assert.That(reader.GetInt32(21), Is.EqualTo(987654));
            Assert.That(reader.GetString(22).TrimEnd(), Is.EqualTo("ESubA 1"));
            Assert.That(reader.GetString(23).TrimEnd(), Is.EqualTo("ESubA 2"));
            Assert.That(reader.GetString(24).TrimEnd(), Is.EqualTo("ESubA 3"));
            Assert.That(reader.GetString(25).TrimEnd(), Is.EqualTo("ESubA 4"));
            Assert.That(reader.GetString(26).TrimEnd(), Is.EqualTo("ESubA 5"));
            Assert.That(reader.GetString(27).TrimEnd(), Is.EqualTo("ESubA 6"));
            Assert.That(reader.GetInt32(28), Is.EqualTo(987654));
            Assert.That(reader.GetInt64(29), Is.EqualTo(2));

            Assert.That(reader.GetString("A").TrimEnd(), Is.EqualTo("Hello world!"));
            Assert.That(reader.GetString("BArray[0]").TrimEnd(), Is.EqualTo("First"));
            Assert.That(reader.GetString("BArray[1]").TrimEnd(), Is.EqualTo("Second"));
            Assert.That(reader.GetString("BArray[2]").TrimEnd(), Is.EqualTo("Third"));
            Assert.That(reader.GetString("BArray[3]").TrimEnd(), Is.EqualTo("Fourth"));
            Assert.That(reader.GetString("BArray[4]").TrimEnd(), Is.EqualTo("Fifth"));
            Assert.That(reader.GetString("BArray[5]").TrimEnd(), Is.EqualTo("Sixth"));
            Assert.That(reader.GetInt32("CArray2_3[0]"), Is.EqualTo(1));
            Assert.That(reader.GetInt32("CArray2_3[1]"), Is.EqualTo(2));
            Assert.That(reader.GetInt32("CArray2_3[2]"), Is.EqualTo(3));
            Assert.That(reader.GetInt32("CArray2_3[3]"), Is.EqualTo(4));
            Assert.That(reader.GetInt32("CArray2_3[4]"), Is.EqualTo(5));
            Assert.That(reader.GetInt32("CArray2_3[5]"), Is.EqualTo(6));
            Assert.That(reader.GetInt32("DGroup.DSubA"), Is.EqualTo(123456));
            Assert.That(reader.GetString("DGroup.DSubB").TrimEnd(), Is.EqualTo("Field B in Group D"));
            Assert.That(reader.GetString("EGroupArray[0].ESubAArray[0]").TrimEnd(), Is.EqualTo("ESubA 1"));
            Assert.That(reader.GetString("EGroupArray[0].ESubAArray[1]").TrimEnd(), Is.EqualTo("ESubA 2"));
            Assert.That(reader.GetString("EGroupArray[0].ESubAArray[2]").TrimEnd(), Is.EqualTo("ESubA 3"));
            Assert.That(reader.GetString("EGroupArray[0].ESubAArray[3]").TrimEnd(), Is.EqualTo("ESubA 4"));
            Assert.That(reader.GetString("EGroupArray[0].ESubAArray[4]").TrimEnd(), Is.EqualTo("ESubA 5"));
            Assert.That(reader.GetString("EGroupArray[0].ESubAArray[5]").TrimEnd(), Is.EqualTo("ESubA 6"));
            Assert.That(reader.GetInt32("EGroupArray[0].ESubB"), Is.EqualTo(987654));
            Assert.That(reader.GetString("EGroupArray[1].ESubAArray[0]").TrimEnd(), Is.EqualTo("ESubA 1"));
            Assert.That(reader.GetString("EGroupArray[1].ESubAArray[1]").TrimEnd(), Is.EqualTo("ESubA 2"));
            Assert.That(reader.GetString("EGroupArray[1].ESubAArray[2]").TrimEnd(), Is.EqualTo("ESubA 3"));
            Assert.That(reader.GetString("EGroupArray[1].ESubAArray[3]").TrimEnd(), Is.EqualTo("ESubA 4"));
            Assert.That(reader.GetString("EGroupArray[1].ESubAArray[4]").TrimEnd(), Is.EqualTo("ESubA 5"));
            Assert.That(reader.GetString("EGroupArray[1].ESubAArray[5]").TrimEnd(), Is.EqualTo("ESubA 6"));
            Assert.That(reader.GetInt32("EGroupArray[1].ESubB"), Is.EqualTo(987654));
            Assert.That(reader.GetInt64("__record_number"), Is.EqualTo(2));

            // -- end

            Assert.That(reader.Read(), Is.False);
        }
    }
}
