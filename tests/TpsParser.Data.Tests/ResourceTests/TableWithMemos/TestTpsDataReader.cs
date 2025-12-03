using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TpsParser.Data.Tests.ResourceTests.TableWithMemos;

internal sealed class TestTpsDataReader
{
    [Test]
    public void Foo()
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

            Assert.That(reader.GetString(0), Is.EqualTo("Joe Smith"));
            Assert.That(reader.GetDateTime(1), Is.EqualTo(new DateTime(2016, 02, 09)));
            Assert.That(reader.GetString(2), Is.EqualTo("He also likes sushi"));
            Assert.That(reader.GetString(3), Is.EqualTo("Joe is a great guy to work with."));
            Assert.That(reader.GetInt64(4), Is.EqualTo(2));

            Assert.That(reader.GetString("name"), Is.EqualTo("Joe Smith"));
            Assert.That(reader.GetDateTime("date"), Is.EqualTo(new DateTime(2016, 02, 09)));
            Assert.That(reader.GetString("additional notes"), Is.EqualTo("He also likes sushi"));
            Assert.That(reader.GetString("notes"), Is.EqualTo("Joe is a great guy to work with."));
            Assert.That(reader.GetInt64("__record_number"), Is.EqualTo(2));

            // -- record 2

            Assert.That(reader.Read(), Is.True);
            Assert.That(reader.FieldCount, Is.EqualTo(5));
            Assert.That(reader.VisibleFieldCount, Is.EqualTo(5));
            Assert.That(reader.RecordsAffected, Is.EqualTo(-1));
            Assert.That(reader.HasRows, Is.True);
            Assert.That(reader.Depth, Is.Zero);

            Assert.That(reader.GetString(0), Is.EqualTo("Jane Jones"));
            Assert.That(reader.GetDateTime(1), Is.EqualTo(new DateTime(2019, 08, 22)));
            Assert.That(reader.GetString(2), Is.EqualTo("She doesn't like sushi as much as Joe."));
            Assert.That(reader.GetString(3), Is.EqualTo("Jane knows how to make a great pot of coffee."));
            Assert.That(reader.GetInt64(4), Is.EqualTo(3));

            Assert.That(reader.GetString("name"), Is.EqualTo("Jane Jones"));
            Assert.That(reader.GetDateTime("date"), Is.EqualTo(new DateTime(2019, 08, 22)));
            Assert.That(reader.GetString("additional notes"), Is.EqualTo("She doesn't like sushi as much as Joe."));
            Assert.That(reader.GetString("notes"), Is.EqualTo("Jane knows how to make a great pot of coffee."));
            Assert.That(reader.GetInt64("__record_number"), Is.EqualTo(3));
        }


    }
}
