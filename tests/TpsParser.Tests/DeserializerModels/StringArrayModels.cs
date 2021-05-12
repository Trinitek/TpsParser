﻿using System.Collections.Generic;
using TpsParser.Tps.Type;

namespace TpsParser.Tests.DeserializerModels
{
    public class TpsObjectIEnumerableStringModel
    {
        [TpsField("BArray")]
        public IEnumerable<TpsObject> Strings { get; set; }
    }

    public class TpsObjectIReadOnlyListStringModel
    {
        [TpsField("BArray")]
        public IReadOnlyList<TpsObject> Strings { get; set; }
    }

    public class TpsStringIEnumerableStringModel
    {
        [TpsField("BArray")]
        public IEnumerable<TpsString> Strings { get; set; }
    }

    public class TpsStringIReadOnlyListStringModel
    {
        [TpsField("BArray")]
        public IReadOnlyList<TpsString> Strings { get; set; }
    }

    public class StringIEnumerableStringModel
    {
        [TpsField("BArray")]
        public IEnumerable<string> Strings { get; set; }
    }

    public class StringIReadOnlyListStringModel
    {
        [TpsField("BArray")]
        public IReadOnlyList<string> Strings { get; set; }
    }

    public class TrimmedStringIEnumerableStringModel
    {
        [StringOptions("BArray")]
        public IEnumerable<string> Strings { get; set; }
    }

    public class TrimmedStringIReadOnlyListStringModel
    {
        [StringOptions("BArray")]
        public IReadOnlyList<string> Strings { get; set; }
    }

    public class BooleanIEnumerableStringModel
    {
        [BooleanOptions("BArray")]
        public IEnumerable<bool> Strings { get; set; }
    }

    public class BooleanIReadOnlyListStringModel
    {
        [BooleanOptions("BArray")]
        public IReadOnlyList<string> Strings { get; set; }
    }
}
