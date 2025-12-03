using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace TpsParser;

/// <summary>
/// Represents a file structure that encapsulates a table's schema.
/// </summary>
public sealed record TableDefinition
{
    /// <summary>
    /// Gets the minimum required TopSpeed database driver version required to read the table.
    /// </summary>
    public int MinimumDriverVersion { get; init; }

    /// <summary>
    /// Gets the number of bytes in each record.
    /// </summary>
    public int RecordLength { get; init; }

    /// <summary>
    /// Gets the field definitions for this table. For <c>MEMO</c>s and <c>BLOB</c>s, see <see cref="Memos"/>.
    /// </summary>
    public required ImmutableArray<FieldDefinition> Fields { get; init; }

    /// <summary>
    /// Gets the <c>MEMO</c> and <c>BLOB</c> definitions for this table. The index of each definition corresponds to <see cref="MemoRecordPayload.DefinitionIndex"/>.
    /// </summary>
    public required ImmutableArray<MemoDefinition> Memos { get; init; }

    /// <summary>
    /// Gets the index definitions for this table. The index of each definition corresponds to <see cref="IndexRecordPayload.DefinitionIndex"/>.
    /// </summary>
    public required ImmutableArray<IndexDefinition> Indexes { get; init; }

    /// <summary>
    /// Creates a new <see cref="TableDefinition"/> using the given data reader.
    /// </summary>
    public static TableDefinition Parse(TpsRandomAccess rx)
    {
        ArgumentNullException.ThrowIfNull(rx);

        short DriverVersion = rx.ReadShortLE();
        short RecordLength = rx.ReadShortLE();

        int fieldCount = rx.ReadShortLE();
        int memoCount = rx.ReadShortLE();
        int indexCount = rx.ReadShortLE();

        List<FieldDefinition> fields = new(fieldCount);
        List<MemoDefinition> memos = new(memoCount);
        List<IndexDefinition> indexes = new(indexCount);

        for (int i = 0; i < fieldCount; i++)
        {
            var fdr = FieldDefinition.Parse(rx);

            fields.Add(fdr);
        }
        for (int i = 0; i < memoCount; i++)
        {
            var mdr = MemoDefinition.Parse(rx);

            memos.Add(mdr);
        }
        for (int i = 0; i < indexCount; i++)
        {
            var idr = IndexDefinition.Parse(rx);

            indexes.Add(idr);
        }

        return new TableDefinition
        {
            MinimumDriverVersion = DriverVersion,
            RecordLength = RecordLength,
            Fields = [..fields],
            Memos = [..memos],
            Indexes = [..indexes]
        };
    }
}
