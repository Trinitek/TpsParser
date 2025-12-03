using System;
using System.Collections.Generic;
using System.Text;

namespace TpsParser;

internal static class ClaStringCommon
{
    public static bool ToBoolean(string? stringValue, ReadOnlyMemory<byte>? contentValue, Encoding encoding)
    {
        if (stringValue is not null)
        {
            return stringValue.Length > 0 && stringValue.AsSpan().ContainsAnyExcept(' ');
        }

        if (contentValue is null)
        {
            throw new InvalidOperationException("Both StringValue and ContentValue are null.");
        }

        if (contentValue.Value.Length == 0)
        {
            return false;
        }

        // We need to determine the byte code used for SPACE ' '.
        // This covers odd encodings like EBCDIC where it's at 0x40 instead of like ASCII at 0x20.

        int spaceByteCount = encoding.GetByteCount(" ");

        if (spaceByteCount > 1)
        {
            throw new ArgumentException($"Specified encoding decodes SPACE character into {spaceByteCount} bytes. Only single-byte encodings are supported.");
        }

        Span<byte> spaceBytes = stackalloc byte[1];

        encoding.GetBytes(" ", spaceBytes);

        return contentValue.Value.Span.ContainsAnyExcept(spaceBytes[0]);
    }

    public static string ToString(
        string? stringValue,
        ReadOnlyMemory<byte>? contentValue,
        Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(encoding);

        if (stringValue is not null)
        {
            return stringValue;
        }

        if (contentValue is null)
        {
            throw new InvalidOperationException("Both StringValue and ContentValue are null.");
        }

        return encoding.GetString(contentValue.Value.Span);
    }

    public static bool Equals(
        string? stringValueA,
        ReadOnlyMemory<byte>? contentValueA,
        string? stringValueB,
        ReadOnlyMemory<byte>? contentValueB)
    {
        return (stringValueA == stringValueB)
            && (contentValueA.HasValue == contentValueB.HasValue)
            && (contentValueA.HasValue is false || contentValueA.Value.Span.SequenceEqual(contentValueB!.Value.Span));
    }

    public static int GetHashCode(
        int seed,
        string? stringValue,
        ReadOnlyMemory<byte>? contentValue)
    {
        return seed + (stringValue is not null
            ? EqualityComparer<string>.Default.GetHashCode(stringValue)
            : contentValue!.Value.Length);
    }
}
