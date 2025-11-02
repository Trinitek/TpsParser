namespace TpsParser.Tps;

/// <summary>
/// Represents a range of offsets for where a <see cref="TpsPage"/> starts and stops in the file.
/// </summary>
/// <param name="StartOffset"></param>
/// <param name="EndOffset"></param>
public sealed record TpsPageRange(int StartOffset, int EndOffset);
