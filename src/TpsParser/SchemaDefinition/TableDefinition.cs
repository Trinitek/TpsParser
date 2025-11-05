using System;
using System.Collections.Generic;
using System.Linq;
using TpsParser.Tps;
using TpsParser.TypeModel;

namespace TpsParser;

/// <summary>
/// Represents a file structure that encapsulates a table's schema.
/// </summary>
public sealed record TableDefinition
{
    /// <summary>
    /// Gets the Clarion database driver version that created the table.
    /// </summary>
    public int DriverVersion { get; init; }

    /// <summary>
    /// Gets the number of bytes in each record.
    /// </summary>
    public int RecordLength { get; init; }

    /// <summary>
    /// Gets the field definitions for this table. For <c>MEMO</c>s s and <c>BLOB</c>s, see <see cref="Memos"/>.
    /// </summary>
    public required IReadOnlyList<FieldDefinition> Fields { get; init; }

    /// <summary>
    /// Gets the <c>MEMO</c> and <c>BLOB</c> definitions for this table. The index of each definition corresponds to <see cref="MemoRecordPayload.DefinitionIndex"/>.
    /// </summary>
    public required IReadOnlyList<MemoDefinition> Memos { get; init; }

    /// <summary>
    /// Gets the index definitions for this table. The index of each definition corresponds to <see cref="IndexRecordPayload.DefinitionIndex"/>.
    /// </summary>
    public required IReadOnlyList<IndexDefinition> Indexes { get; init; }

    /// <summary>
    /// Creates a new <see cref="TableDefinition"/> by parsing the data from the given <see cref="TpsRandomAccess"/> reader.
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
            DriverVersion = DriverVersion,
            RecordLength = RecordLength,
            Fields = fields.AsReadOnly(),
            Memos = memos.AsReadOnly(),
            Indexes = indexes.AsReadOnly()
        };
    }

    /// <summary>
    /// Gets a list of field values by parsing the given byte reader.
    /// </summary>
    /// <param name="rx"></param>
    /// <returns></returns>
    public IReadOnlyList<IClaObject> ParseFields(TpsRandomAccess rx)
    {
        var values = new List<IClaObject>(Fields.Count());

        foreach (var field in Fields)
        {
            if (field.IsArray)
            {
                int fieldSize = RecordLength / field.ElementCount;

                for (int i = 0; i < field.ElementCount; i++)
                {
                    values.Add(ParseField(field.TypeCode, fieldSize, field, rx));
                }
            }
            else
            {
                values.Add(ParseField(field.TypeCode, field.Length, field, rx));
            }
        }

        return values.AsReadOnly();
    }

    private IClaObject ParseField(ClaTypeCode type, int length, FieldDefinition fieldDefinitionRecord, TpsRandomAccess rx)
    {
        if (fieldDefinitionRecord == null)
        {
            throw new ArgumentNullException(nameof(fieldDefinitionRecord));
        }

        if (rx == null)
        {
            throw new ArgumentNullException(nameof(rx));
        }

        switch (type)
        {
            case ClaTypeCode.Byte:
                AssertEqual(1, length);
                return rx.ReadClaByte();
            case ClaTypeCode.Short:
                AssertEqual(2, length);
                return rx.ReadClaShort();
            case ClaTypeCode.UShort:
                AssertEqual(2, length);
                return rx.ReadClaUnsignedShort();
            case ClaTypeCode.Date:
                return rx.ReadClaDate();
            case ClaTypeCode.Time:
                return rx.ReadClaTime();
            case ClaTypeCode.Long:
                AssertEqual(4, length);
                return rx.ReadClaLong();
            case ClaTypeCode.ULong:
                AssertEqual(4, length);
                return rx.ReadClaUnsignedLong();
            case ClaTypeCode.SReal:
                AssertEqual(4, length);
                return rx.ReadClaFloat();
            case ClaTypeCode.Real:
                AssertEqual(8, length);
                return rx.ReadClaDouble();
            case ClaTypeCode.Decimal:
                return rx.ReadClaDecimal(length: length, digitsAfterDecimalPoint: fieldDefinitionRecord.BcdDigitsAfterDecimalPoint);
            case ClaTypeCode.FString:
                return rx.ReadClaFString(length: fieldDefinitionRecord.StringLength);
            case ClaTypeCode.CString:
                return rx.ReadClaCString();
            case ClaTypeCode.PString:
                return rx.ReadClaPString();
            case ClaTypeCode.Group:
                //return new TpsGroup(rx, length);
            default:
                throw new ArgumentException($"Unsupported type {type} ({length})", nameof(type));
        }
    }

    private void AssertEqual(int reference, int value)
    {
        if (reference != value)
        {
            throw new ArgumentException($"{reference} != {value}");
        }
    }
}
