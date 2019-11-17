using NUnit.Framework;
using System.Linq;

namespace TpsParser.Tests.ParserIntegrations
{
    [TestFixture]
    public class ArrayOfGroups
    {
        private TpsParser GetParser() => new TpsParser("Resources/array-of-groups.tps");

        [Test]
        public void ShouldBuildTable()
        {
            using (var parser = GetParser())
            {
                var table = parser.BuildTable(ignoreErrors: false);

                Assert.AreEqual(1, table.Rows.Count());
                Assert.AreEqual(5, table.Rows.First().Values.Count());
            }
        }
    }
}
