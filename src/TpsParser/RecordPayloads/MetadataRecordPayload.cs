using System;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;

namespace TpsParser;

/// <summary>
/// A record payload that contains metadata about other records.
/// </summary>
public readonly record struct MetadataRecordPayload : IRecordPayload, IPayloadTableNumber
{
    /// <inheritdoc/>
    public required ReadOnlyMemory<byte> PayloadData { get; init; }

    /// <inheritdoc cref="IPayloadTableNumber.TableNumber"/>
    public int TableNumber => BinaryPrimitives.ReadInt32BigEndian(PayloadData.Span[0..]);

    /// <summary>
    /// Gets the type of record that this metadata describes.
    /// </summary>
    public RecordPayloadType AboutType => (RecordPayloadType)PayloadData.Span[5];

    /// <summary>
    /// Returns <see langword="true"/> if <see cref="Content"/> contains metadata about data records.
    /// </summary>
    public bool IsAboutData => AboutType == RecordPayloadType.Data;

    /// <summary>
    /// Returns <see langword="true"/> if <see cref="Content"/> contains metadata about an index.
    /// </summary>
    public bool IsAboutIndex => AboutType < RecordPayloadType.Data;

    /// <summary>
    /// Gets the memory region of the content in this entry.
    /// </summary>
    /// <remarks>
    /// Reverse-engineering notes: Data and Index metadata both include an extra 4 zero-bytes at the end of the content for all inspected files.
    /// Not sure what this is.
    /// </remarks>
    public ReadOnlyMemory<byte> Content => PayloadData[6..];

    /// <summary>
    /// Attempts to parse the content as metadata about data records.
    /// </summary>
    /// <param name="metadata"></param>
    /// <returns></returns>
    public bool TryParseContentAsDataMetadata([NotNullWhen(true)] out DataMetadata? metadata)
    {
        if (!IsAboutData)
        {
            metadata = null;
            return false;
        }

        var span = Content.Span;

        int recordCount = BinaryPrimitives.ReadInt32LittleEndian(span);

        metadata = new DataMetadata(
            DataRecordCount: recordCount);

        return true;
    }

    /// <summary>
    /// Attempts to parse the content as metadata about an index.
    /// </summary>
    /// <param name="metadata"></param>
    /// <returns></returns>
    public bool TryParseContentAsIndexMetadata([NotNullWhen(true)] out IndexMetadata? metadata)
    {
        if (!IsAboutIndex)
        {
            metadata = null;
            return false;
        }

        var span = Content.Span;

        int recordCount = BinaryPrimitives.ReadInt32LittleEndian(span);

        metadata = new IndexMetadata(
            DataRecordCount: recordCount);

        return true;
    }
}

/// <summary>
/// Encapsulates metadata about data records in a table.
/// </summary>
/// <param name="DataRecordCount">The number of data records in the table.</param>
public sealed record DataMetadata(int DataRecordCount);

/// <summary>
/// Encapsulates metadata about an index.
/// </summary>
/// <param name="DataRecordCount">The number of data records in this index.</param>
public sealed record IndexMetadata(int DataRecordCount);
