using NUnit.Framework;
using System.Collections.Generic;
using TpsParser.Tests.DeserializerModels;
using TpsParser.Tps.Type;

namespace TpsParser.Tests
{
    public partial class RowTest
    {
        [TestFixture]
        public class DeserializeLong
        {
            [Test]
            public void ShouldDeserializeLong()
            {
                var row = BuildRow(1, ("Count", new TpsLong(12)));

                var deserialized = row.Deserialize<LongModel>();

                Assert.That(deserialized.Count, Is.EqualTo(12));
            }

            [TestCaseSource(typeof(BadFieldObjectData), nameof(BadFieldObjectData.Data))]
            public void ShouldThrowWhenDeserializing(TpsObject tpsObject)
            {
                var row = BuildRow(1, ("Count", tpsObject));

                Assert.Throws<TpsParserException>(() => row.Deserialize<LongModel>());
            }

            private class BadFieldObjectData
            {
                public static IEnumerable<TpsObject> Data
                {
                    get
                    {
                        yield return new TpsString("12");
                        yield return new TpsCString("12");
                        yield return new TpsPString("12");
                    }
                }
            }
        }
    }
}
