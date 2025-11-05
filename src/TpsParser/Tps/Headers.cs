using System;
using System.ComponentModel;
using TpsParser.Tps.Record;

namespace TpsParser.Tps;

/// <summary>
/// Encapsulates information that describes a record.
/// </summary>
public interface IHeader
{
    /// <summary>
    /// Gets the reported record type.
    /// </summary>
    RecordPayloadType PayloadType { get; }

    /// <summary>
    /// Gets the table number to which the header belongs.
    /// </summary>
    int TableNumber { get; }
}

/// <summary>
/// Represents the type of record and what information it describes.
/// </summary>
public enum RecordPayloadType : byte
{
    /// <summary>
    /// A section that contains a database record.
    /// </summary>
    Data        = 0xF3,

    /// <summary>
    /// A section that contains metadata about other records.
    /// </summary>
    Metadata    = 0xF6,

    /// <summary>
    /// A section that contains a <see cref="TableDefinition"/>.
    /// Sufficiently large table definitions will have two or more <see cref="TableDef"/> records.
    /// </summary>
    TableDef    = 0xFA,

    /// <summary>
    /// A section that contains an index.
    /// </summary>
    Index       = 0xFB,

    /// <summary>
    /// A section that contains a <c>MEMO</c> or <c>BLOB</c>.
    /// Sufficiently large objects will have two or more <see cref="Memo"/> records.
    /// </summary>
    Memo        = 0xFC,

    /// <summary>
    /// A section that contains the name of the table.
    /// </summary>
    TableName   = 0xFE,
}

/// <summary>
/// Represents a slice of header data that is common to most <see cref="IHeader"/> types.
/// </summary>
/// <param name="Type">The type of record described by the <see cref="TpsRecord"/> object.</param>
/// <param name="TableNumber">The table identifier to which the <see cref="TpsRecord"/> applies.</param>
public sealed record PreHeader(RecordPayloadType Type, int TableNumber)
{
    /// <summary>
    /// Creates a new PreHeader using the data reader.
    /// </summary>
    /// <param name="rx"></param>
    /// <param name="readTableNumber">
    /// True if the reader should read a table number from the header data.
    /// Some headers don't have an associated table number.
    /// </param>
    /// <returns></returns>
    public static PreHeader Parse(TpsRandomAccess rx, bool readTableNumber)
    {
        ArgumentNullException.ThrowIfNull(rx);

        int tableNumber = 0;

        if (readTableNumber)
        {
            tableNumber = rx.ReadLongBE();
        }

        RecordPayloadType type = (RecordPayloadType)rx.ReadByte();

        return new(
            Type: type,
            TableNumber: tableNumber);
    }
}

/// <summary>
/// Encapsulates information about a particular <see cref="IndexDefinition"/> or <see cref="IndexRecord"/>.
/// </summary>
public sealed record IndexHeader : IHeader
{
    /// <inheritdoc cref="IHeader.PayloadType"/>
    public RecordPayloadType PayloadType { get; init; }

    /// <inheritdoc cref="IHeader.TableNumber"/>
    public int TableNumber { get; init; }

    /// <summary>
    /// Gets the index identifier.
    /// </summary>
    public byte IndexNumber => (byte)PayloadType;

    /// <summary>
    /// Creates a new <see cref="IndexHeader"/> from the given preheader and data reader.
    /// </summary>
    /// <param name="preHeader"></param>
    /// <param name="rx"></param>
    /// <returns></returns>
    public static IndexHeader Parse(PreHeader preHeader, TpsRandomAccess rx)
    {
        ArgumentNullException.ThrowIfNull(rx);

        return new IndexHeader
        {
            PayloadType = preHeader.Type,
            TableNumber = preHeader.TableNumber,
        };
    }
}

/// <summary>
/// Encapsulates information about a particular <see cref="MemoRecord"/>.
/// </summary>
public sealed class MemoHeader : IHeader
{
    /// <inheritdoc cref="IHeader.PayloadType"/>
    public RecordPayloadType PayloadType { get; init; }

    /// <inheritdoc cref="IHeader.TableNumber"/>
    public int TableNumber { get; init; }

    /// <summary>
    /// Gets the number of the <see cref="IDataRecord"/> that owns this memo.
    /// </summary>
    public int OwningRecord { get; init; }

    /// <summary>
    /// Gets the sequence number of the memo when the memo is segmented into multiple records. The first segment is 0.
    /// </summary>
    public ushort SequenceNumber { get; init; }

    /// <summary>
    /// Gets the index at which the memo appears in the record. Corresponds to the index number of <see cref="TableDefinition.Memos"/>.
    /// </summary>
    public byte MemoIndex { get; init; }

    /// <summary>
    /// Creates a new <see cref="MemoHeader"/> from the given preheader and data reader.
    /// </summary>
    /// <param name="preHeader"></param>
    /// <param name="rx"></param>
    /// <returns></returns>
    public static MemoHeader Parse(PreHeader preHeader, TpsRandomAccess rx)
    {
        ArgumentNullException.ThrowIfNull(preHeader);
        ArgumentNullException.ThrowIfNull(rx);

        int owningRecord = rx.ReadLongBE();
        byte memoIndex = rx.ReadByte();
        ushort sequenceNumber = rx.ReadUnsignedShortBE();

        return new MemoHeader
        {
            PayloadType = preHeader.Type,
            TableNumber = preHeader.TableNumber,
            OwningRecord = owningRecord,
            SequenceNumber = sequenceNumber,
            MemoIndex = memoIndex
        };
    }
}

/// <summary>
/// Encapsulates information about a particular metadata record.
/// </summary>
public sealed class MetadataHeader : IHeader
{
    /// <inheritdoc cref="IHeader.PayloadType"/>
    public RecordPayloadType PayloadType { get; init; }

    /// <inheritdoc cref="IHeader.TableNumber"/>
    public int TableNumber { get; init; }

    /// <summary>
    /// Gets the record type that this metadata describes.
    /// </summary>
    public RecordPayloadType AboutType { get; init; }

    public bool IsAboutData => AboutType == RecordPayloadType.Data;

    public bool IsAboutKeyOrIndex => AboutType < RecordPayloadType.Data;

    /// <summary>
    /// Creates a new <see cref="MetadataHeader"/> from the given preheader and data reader.
    /// </summary>
    /// <param name="preHeader"></param>
    /// <param name="rx"></param>
    /// <returns></returns>
    public static MetadataHeader Parse(PreHeader preHeader, TpsRandomAccess rx)
    {
        ArgumentNullException.ThrowIfNull(preHeader);
        ArgumentNullException.ThrowIfNull(rx);

        RecordPayloadType aboutType = (RecordPayloadType)rx.ReadByte();

        return new MetadataHeader
        {
            PayloadType = preHeader.Type,
            TableNumber = preHeader.TableNumber,
            AboutType = aboutType
        };
    }
}

/// <summary>
/// Encapsulates information about a particular <see cref="TableDefinition"/>.
/// </summary>
public sealed class TableDefinitionHeader : IHeader
{
    /// <inheritdoc cref="IHeader.PayloadType"/>
    public RecordPayloadType PayloadType { get; init; }

    /// <inheritdoc cref="IHeader.TableNumber"/>
    public int TableNumber { get; init; }

    /// <summary>
    /// Gets the block identifier in which the table is located.
    /// </summary>
    public ushort Block { get; init; }

    /// <summary>
    /// Creates a new <see cref="TableDefinitionHeader"/> from the given preheader and data reader.
    /// </summary>
    /// <param name="preHeader"></param>
    /// <param name="rx"></param>
    /// <returns></returns>
    public static TableDefinitionHeader Parse(PreHeader preHeader, TpsRandomAccess rx)
    {
        ArgumentNullException.ThrowIfNull(preHeader);
        ArgumentNullException.ThrowIfNull(rx);

        ushort block = rx.ReadUnsignedShortLE();

        return new TableDefinitionHeader
        {
            PayloadType = preHeader.Type,
            TableNumber = preHeader.TableNumber,
            Block = block
        };
    }
}

/// <summary>
/// Encapsulates information about a particular <see cref="TableNameRecord"/>.
/// </summary>
public sealed class TableNameHeader : IHeader
{
    /// <inheritdoc cref="IHeader.PayloadType"/>
    public RecordPayloadType PayloadType { get; init; }

    /// <summary>
    /// Not supported on table name headers; see the associated <see cref="TableNameRecord.TableNumber"/> instead.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public int TableNumber { get; } = 0;

    /// <summary>
    /// Gets the name of the table.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Creates a new <see cref="TableNameHeader"/> from the given preheader and data reader.
    /// </summary>
    /// <param name="preHeader"></param>
    /// <param name="rx"></param>
    /// <returns></returns>
    public static TableNameHeader Parse(PreHeader preHeader, TpsRandomAccess rx)
    {
        ArgumentNullException.ThrowIfNull(preHeader);
        ArgumentNullException.ThrowIfNull(rx);

        string name = rx.ReadFixedLengthString(rx.Length - rx.Position);

        return new TableNameHeader
        {
            PayloadType = preHeader.Type,
            Name = name
        };
    }
}

/// <summary>
/// Encapsulates information about a particular <see cref="DataRecord"/>.
/// </summary>
public sealed class DataHeader : IHeader
{
    /// <inheritdoc cref="IHeader.PayloadType"/>
    public RecordPayloadType PayloadType { get; init; }

    /// <inheritdoc cref="IHeader.TableNumber"/>
    public int TableNumber { get; init; }

    /// <summary>
    /// Gets the record number to which this data is associated.
    /// </summary>
    public int RecordNumber { get; init; }

    /// <summary>
    /// Creates a new <see cref="DataHeader"/> from the given preheader and data reader.
    /// </summary>
    /// <param name="preHeader"></param>
    /// <param name="rx"></param>
    /// <returns></returns>
    public static DataHeader Parse(PreHeader preHeader, TpsRandomAccess rx)
    {
        ArgumentNullException.ThrowIfNull(preHeader);
        ArgumentNullException.ThrowIfNull(rx);

        int recordNumber = rx.ReadLongBE();

        return new DataHeader
        {
            PayloadType = preHeader.Type,
            TableNumber = preHeader.TableNumber,
            RecordNumber = recordNumber
        };
    }
}
