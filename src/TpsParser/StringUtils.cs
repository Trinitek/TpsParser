namespace TpsParser;

internal static class StringUtils
{
    public static string ToHex8(int value) => $"{value:X8}";

    public static string ToHex4(int value) => $"{value:X4}";

    public static string ToHex2(int value) => $"{value:X2}";
}
