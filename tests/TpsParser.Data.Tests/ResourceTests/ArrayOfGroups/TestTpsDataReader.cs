using NUnit.Framework;
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
}
