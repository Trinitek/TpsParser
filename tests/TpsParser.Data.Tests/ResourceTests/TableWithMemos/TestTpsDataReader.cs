using NUnit.Framework;
using System;
using System.Data;
using System.Text;

namespace TpsParser.Data.Tests.ResourceTests.TableWithMemos;

internal sealed class TestTpsDataReader
{
    [Test]
    public void FullRead()
    {
        var csBuilder = new TpsConnectionStringBuilder
        {
            Folder = "Resources"
        };

        using var connection = new TpsDbConnection(csBuilder.ConnectionString);

        connection.Open();

        using var command = new TpsDbCommand("select * from table-with-memos", connection);

        using var reader = command.ExecuteReader();

        using (Assert.EnterMultipleScope())
        {
            // -- record 1

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.FieldCount, Is.EqualTo(5));
            Assert.That(reader.VisibleFieldCount, Is.EqualTo(5));
            Assert.That(reader.RecordsAffected, Is.EqualTo(-1));
            Assert.That(reader.HasRows, Is.True);
            Assert.That(reader.Depth, Is.Zero);

            Assert.That(reader.GetString(0).TrimEnd(), Is.EqualTo("Joe Smith"));
            Assert.That(reader.GetValue(1), Is.EqualTo(new DateOnly(2016, 02, 09)));
            Assert.That(reader.GetString(2), Is.EqualTo("He also likes sushi."));
            Assert.That(reader.GetString(3), Is.EqualTo("Joe is a great guy to work with."));
            Assert.That(reader.GetInt64(4), Is.EqualTo(2));

            Assert.That(reader.GetString("name").TrimEnd(), Is.EqualTo("Joe Smith"));
            Assert.That(reader.GetValue("date"), Is.EqualTo(new DateOnly(2016, 02, 09)));
            Assert.That(reader.GetString("additionalnotes"), Is.EqualTo("He also likes sushi."));
            Assert.That(reader.GetString("notes"), Is.EqualTo("Joe is a great guy to work with."));
            Assert.That(reader.GetInt64("__record_number"), Is.EqualTo(2));

            Assert.That(reader.IsDBNull(0), Is.False);
            Assert.That(reader.IsDBNull(1), Is.False);
            Assert.That(reader.IsDBNull(2), Is.False);
            Assert.That(reader.IsDBNull(3), Is.False);
            Assert.That(reader.IsDBNull(4), Is.False);

            Assert.That(reader.IsDBNull("name"), Is.False);
            Assert.That(reader.IsDBNull("date"), Is.False);
            Assert.That(reader.IsDBNull("additionalnotes"), Is.False);
            Assert.That(reader.IsDBNull("notes"), Is.False);
            Assert.That(reader.IsDBNull("__record_number"), Is.False);

            // -- record 2

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.FieldCount, Is.EqualTo(5));
            Assert.That(reader.VisibleFieldCount, Is.EqualTo(5));
            Assert.That(reader.RecordsAffected, Is.EqualTo(-1));
            Assert.That(reader.HasRows, Is.True);
            Assert.That(reader.Depth, Is.Zero);

            Assert.That(reader.GetString(0).TrimEnd(), Is.EqualTo("Jane Jones"));
            Assert.That(reader.GetValue(1), Is.EqualTo(new DateOnly(2019, 08, 22)));
            Assert.That(reader.GetString(2), Is.EqualTo("She doesn't like sushi as much as Joe."));
            Assert.That(reader.GetString(3), Is.EqualTo("Jane knows how to make a great pot of coffee."));
            Assert.That(reader.GetInt64(4), Is.EqualTo(3));

            Assert.That(reader.GetString("name").TrimEnd(), Is.EqualTo("Jane Jones"));
            Assert.That(reader.GetValue("date"), Is.EqualTo(new DateOnly(2019, 08, 22)));
            Assert.That(reader.GetString("additionalnotes"), Is.EqualTo("She doesn't like sushi as much as Joe."));
            Assert.That(reader.GetString("notes"), Is.EqualTo("Jane knows how to make a great pot of coffee."));
            Assert.That(reader.GetInt64("__record_number"), Is.EqualTo(3));

            Assert.That(reader.IsDBNull(0), Is.False);
            Assert.That(reader.IsDBNull(1), Is.False);
            Assert.That(reader.IsDBNull(2), Is.False);
            Assert.That(reader.IsDBNull(3), Is.False);
            Assert.That(reader.IsDBNull(4), Is.False);

            Assert.That(reader.IsDBNull("name"), Is.False);
            Assert.That(reader.IsDBNull("date"), Is.False);
            Assert.That(reader.IsDBNull("additionalnotes"), Is.False);
            Assert.That(reader.IsDBNull("notes"), Is.False);
            Assert.That(reader.IsDBNull("__record_number"), Is.False);

            // -- record 3

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.FieldCount, Is.EqualTo(5));
            Assert.That(reader.VisibleFieldCount, Is.EqualTo(5));
            Assert.That(reader.RecordsAffected, Is.EqualTo(-1));
            Assert.That(reader.HasRows, Is.True);
            Assert.That(reader.Depth, Is.Zero);

            Assert.That(reader.GetString(0).TrimEnd(), Is.EqualTo("John NoNotes"));
            Assert.That(reader.GetValue(1), Is.EqualTo(new DateOnly(2019, 10, 07)));
            Assert.That(reader.GetValue(2), Is.EqualTo(DBNull.Value));
            Assert.That(reader.GetValue(3), Is.EqualTo(DBNull.Value));
            Assert.That(reader.GetInt64(4), Is.EqualTo(4));

            Assert.That(reader.GetString("name").TrimEnd(), Is.EqualTo("John NoNotes"));
            Assert.That(reader.GetValue("date"), Is.EqualTo(new DateOnly(2019, 10, 07)));
            Assert.That(reader.GetValue("additionalnotes"), Is.EqualTo(DBNull.Value));
            Assert.That(reader.GetValue("notes"), Is.EqualTo(DBNull.Value));
            Assert.That(reader.GetInt64("__record_number"), Is.EqualTo(4));

            Assert.That(reader.IsDBNull(0), Is.False);
            Assert.That(reader.IsDBNull(1), Is.False);
            Assert.That(reader.IsDBNull(2), Is.True);
            Assert.That(reader.IsDBNull(3), Is.True);
            Assert.That(reader.IsDBNull(4), Is.False);

            Assert.That(reader.IsDBNull("name"), Is.False);
            Assert.That(reader.IsDBNull("date"), Is.False);
            Assert.That(reader.IsDBNull("additionalnotes"), Is.True);
            Assert.That(reader.IsDBNull("notes"), Is.True);
            Assert.That(reader.IsDBNull("__record_number"), Is.False);

            // -- record 4

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.FieldCount, Is.EqualTo(5));
            Assert.That(reader.VisibleFieldCount, Is.EqualTo(5));
            Assert.That(reader.RecordsAffected, Is.EqualTo(-1));
            Assert.That(reader.HasRows, Is.True);
            Assert.That(reader.Depth, Is.Zero);

            Assert.That(reader.GetString(0).TrimEnd(), Is.EqualTo("Jimmy OneNote"));
            Assert.That(reader.GetValue(1), Is.EqualTo(new DateOnly(2013, 03, 14)));
            Assert.That(reader.GetValue(2), Is.EqualTo("Has a strange last name."));
            Assert.That(reader.GetValue(3), Is.EqualTo(DBNull.Value));
            Assert.That(reader.GetInt64(4), Is.EqualTo(5));

            Assert.That(reader.GetString("name").TrimEnd(), Is.EqualTo("Jimmy OneNote"));
            Assert.That(reader.GetValue("date"), Is.EqualTo(new DateOnly(2013, 03, 14)));
            Assert.That(reader.GetValue("additionalnotes"), Is.EqualTo("Has a strange last name."));
            Assert.That(reader.GetValue("notes"), Is.EqualTo(DBNull.Value));
            Assert.That(reader.GetInt64("__record_number"), Is.EqualTo(5));

            Assert.That(reader.IsDBNull(0), Is.False);
            Assert.That(reader.IsDBNull(1), Is.False);
            Assert.That(reader.IsDBNull(2), Is.False);
            Assert.That(reader.IsDBNull(3), Is.True);
            Assert.That(reader.IsDBNull(4), Is.False);

            Assert.That(reader.IsDBNull("name"), Is.False);
            Assert.That(reader.IsDBNull("date"), Is.False);
            Assert.That(reader.IsDBNull("additionalnotes"), Is.False);
            Assert.That(reader.IsDBNull("notes"), Is.True);
            Assert.That(reader.IsDBNull("__record_number"), Is.False);

            // -- end

            Assert.That(reader.Read(), Is.False);
        }


    }
}
