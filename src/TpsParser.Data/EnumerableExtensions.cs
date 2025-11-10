using System.Collections.Generic;

namespace TpsParser.Data;

internal static class EnumerableExtensions
{
    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source)
    {
        return source ?? [];
    }
}