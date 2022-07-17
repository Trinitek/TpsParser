using System.Collections.Generic;
using System.Linq;
using TpsParser.Tps.Type;

namespace TpsParser.Tests.RowDeserializer
{
    public static class RowDeserializerExtensions
    {
        public static Row BuildRow(int rowNumber, params (string columnName, ITpsObject value)[] fields) =>
            new Row(
                new DeserializerContext(
                    new StringOptions(),
                    new BooleanOptions()),
                rowNumber,
                new Dictionary<string, ITpsObject>(
                    fields.Select(f => new KeyValuePair<string, ITpsObject>(f.columnName, f.value))));
    }
}
