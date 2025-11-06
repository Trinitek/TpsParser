namespace TpsParser.Tps;

/// <summary>
/// Represents a range of offsets for where a <see cref="TpsBlock"/> starts and stops in the file.
/// </summary>
/// <param name="StartOffset"></param>
/// <param name="EndOffset"></param>
public sealed record TpsBlockDescriptor(int StartOffset, int EndOffset)
{
    /// <summary>
    /// Gets the length of the page in bytes.
    /// </summary>
    public int Length => EndOffset - StartOffset;
}
