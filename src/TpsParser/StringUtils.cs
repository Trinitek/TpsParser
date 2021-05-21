namespace TpsParser
{
    internal static class StringUtils
    {
        public static string ToHex8(int value) => $"{value:X8}";

        public static string ToHex4(int value) => $"{value:X4}";

        public static string ToHex2(int value) => $"{value:X2}";

        public static string NullTrimStart(this string value) => value?.TrimStart();

        public static string NullTrimEnd(this string value) => value?.TrimEnd();

        public static string NullTrim(this string value) => value?.Trim();
    }
}
