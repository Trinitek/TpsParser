namespace TpsParser.Tps;

/// <summary>
/// Represents a range of offsets for where a <see cref="TpsBlock"/> starts and stops in the file.
/// </summary>
/// <param name="StartOffset"></param>
/// <param name="EndOffset"></param>
public sealed record TpsBlockDescriptor(uint StartOffset, uint EndOffset)
{
    /// <summary>
    /// Gets the length of the page in bytes.
    /// </summary>
    public uint Length => EndOffset - StartOffset;
}
