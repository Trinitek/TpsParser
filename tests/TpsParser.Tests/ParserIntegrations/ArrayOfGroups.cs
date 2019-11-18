using NUnit.Framework;
using System.Linq;
using TpsParser.Tests.DeserializerModels;

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

        [Test]
        public void ShouldDeserializeTpsObjectIEnumerableStringModel()
        {
            using (var parser = GetParser())
            {
                var deserialized = parser.Deserialize<TpsObjectIEnumerableStringModel>();

                Assert.AreEqual(1, deserialized.Count());

                var model = deserialized.First();

                CollectionAssert.AreEqual(new string[]
                {
                    "First               ",
                    "Second              ",
                    "Third               ",
                    "Fourth              ",
                    "Fifth               ",
                    "Sixth               "
                },
                model.Strings.Select(s => s.Value));
            }
        }

        [Test]
        public void ShouldDeserializeTpsObjectIReadOnlyListStringModel()
        {
            using (var parser = GetParser())
            {
                var deserialized = parser.Deserialize<TpsObjectIReadOnlyListStringModel>();

                Assert.AreEqual(1, deserialized.Count());

                var model = deserialized.First();

                CollectionAssert.AreEqual(new string[]
                {
                    "First               ",
                    "Second              ",
                    "Third               ",
                    "Fourth              ",
                    "Fifth               ",
                    "Sixth               "
                },
                model.Strings.Select(s => s.Value));
            }
        }

        [Test]
        public void ShouldDeserializeTpsStringIEnumerableStringModel()
        {
            using (var parser = GetParser())
            {
                var deserialized = parser.Deserialize<TpsStringIEnumerableStringModel>();

                Assert.AreEqual(1, deserialized.Count());

                var model = deserialized.First();

                CollectionAssert.AreEqual(new string[]
                {
                    "First               ",
                    "Second              ",
                    "Third               ",
                    "Fourth              ",
                    "Fifth               ",
                    "Sixth               "
                },
                model.Strings.Select(s => s.Value));
            }
        }

        [Test]
        public void ShouldDeserializeTpsStringIReadOnlyListStringModel()
        {
            using (var parser = GetParser())
            {
                var deserialized = parser.Deserialize<TpsStringIReadOnlyListStringModel>();

                Assert.AreEqual(1, deserialized.Count());

                var model = deserialized.First();

                CollectionAssert.AreEqual(new string[]
                {
                    "First               ",
                    "Second              ",
                    "Third               ",
                    "Fourth              ",
                    "Fifth               ",
                    "Sixth               "
                },
                model.Strings.Select(s => s.Value));
            }
        }

        [Test]
        public void ShouldDeserializeStringIEnumerableStringModel()
        {
            using (var parser = GetParser())
            {
                var deserialized = parser.Deserialize<StringIEnumerableStringModel>();

                Assert.AreEqual(1, deserialized.Count());

                var model = deserialized.First();

                CollectionAssert.AreEqual(new string[]
                {
                    "First               ",
                    "Second              ",
                    "Third               ",
                    "Fourth              ",
                    "Fifth               ",
                    "Sixth               "
                },
                model.Strings);
            }
        }

        [Test]
        public void ShouldDeserializeStringIReadOnlyListStringModel()
        {
            using (var parser = GetParser())
            {
                var deserialized = parser.Deserialize<StringIReadOnlyListStringModel>();

                Assert.AreEqual(1, deserialized.Count());

                var model = deserialized.First();

                CollectionAssert.AreEqual(new string[]
                {
                    "First               ",
                    "Second              ",
                    "Third               ",
                    "Fourth              ",
                    "Fifth               ",
                    "Sixth               "
                },
                model.Strings);
            }
        }

        [Test]
        public void ShouldDeserializeTrimmedStringIEnumerableStringModel()
        {
            using (var parser = GetParser())
            {
                var deserialized = parser.Deserialize<TrimmedStringIEnumerableStringModel>();

                Assert.AreEqual(1, deserialized.Count());

                var model = deserialized.First();

                CollectionAssert.AreEqual(new string[]
                {
                    "First",
                    "Second",
                    "Third",
                    "Fourth",
                    "Fifth",
                    "Sixth"
                },
                model.Strings);
            }
        }

        [Test]
        public void ShouldDeserializeTrimmedStringIReadOnlyListStringModel()
        {
            using (var parser = GetParser())
            {
                var deserialized = parser.Deserialize<TrimmedStringIReadOnlyListStringModel>();

                Assert.AreEqual(1, deserialized.Count());

                var model = deserialized.First();

                CollectionAssert.AreEqual(new string[]
                {
                    "First",
                    "Second",
                    "Third",
                    "Fourth",
                    "Fifth",
                    "Sixth"
                },
                model.Strings);
            }
        }

        [Test]
        public void ShouldDeserializeBooleanIEnumerableStringModel()
        {
            using (var parser = GetParser())
            {
                var deserialized = parser.Deserialize<BooleanIEnumerableStringModel>();

                Assert.AreEqual(1, deserialized.Count());

                var model = deserialized.First();

                CollectionAssert.AreEqual(new bool[]
                {
                    true,
                    true,
                    true,
                    true,
                    true,
                    true
                },
                model.Strings);
            }
        }

        [Test]
        public void ShouldDeserializeBooleanIReadOnlyListStringModel()
        {
            using (var parser = GetParser())
            {
                var deserialized = parser.Deserialize<BooleanIReadOnlyListStringModel>();

                Assert.AreEqual(1, deserialized.Count());

                var model = deserialized.First();

                CollectionAssert.AreEqual(new bool[]
                {
                    true,
                    true,
                    true,
                    true,
                    true,
                    true
                },
                model.Strings);
            }
        }
    }
}
