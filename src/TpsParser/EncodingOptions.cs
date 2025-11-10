using System.Text;

namespace TpsParser;

/// <summary>
/// Encapsulates the different text encodings to use when reading strings.
/// </summary>
public sealed record EncodingOptions
{
    /// <summary>
    /// Gets the default options using <see cref="Encoding.Latin1"/>.
    /// </summary>
    public static readonly EncodingOptions Default = new()
    {
        ContentEncoding = Encoding.Latin1,
        MetadataEncoding = Encoding.Latin1
    };

    /// <summary>
    /// The text encoding to use when reading user-defined content in string fields and <c>MEMO</c>s.
    /// </summary>
    public required Encoding ContentEncoding { get; init; }

    /// <summary>
    /// The text encoding to use when reading database metadata structures, such as table names and field names.
    /// </summary>
    public required Encoding MetadataEncoding { get; init; }
}
