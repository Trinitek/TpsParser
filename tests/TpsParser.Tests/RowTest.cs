using System.Collections.Generic;
using System.Linq;
using TpsParser.Tps.Type;

namespace TpsParser.Tests
{
    public partial class RowTest
    {
        private static Row BuildRow(int rowNumber, params (string columnName, TpsObject value)[] fields) =>
            new Row(new DeserializerContext(), rowNumber, new Dictionary<string, TpsObject>(fields.Select(f => new KeyValuePair<string, TpsObject>(f.columnName, f.value))));
    }
}
