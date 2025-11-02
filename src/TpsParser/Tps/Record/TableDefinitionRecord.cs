using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TpsParser.Binary;
using TpsParser.TypeModel;

namespace TpsParser.Tps.Record;

/// <summary>
/// Represents a file structure that encapsulates a table's schema.
/// </summary>
public sealed record TableDefinitionRecord
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
    /// Gets the field definitions for this table. For MEMOs and BLOBs, see <see cref="Memos"/>.
    /// </summary>
    public required IReadOnlyList<FieldDefinitionRecord> Fields { get; init; }

    /// <summary>
    /// Gets the MEMO and BLOB definitions for this table. The index of each definition corresponds to <see cref="Header.IMemoHeader.MemoIndex"/>.
    /// </summary>
    public required IReadOnlyList<MemoDefinitionRecord> Memos { get; init; }

    /// <summary>
    /// Gets the index definitions for this table.
    /// </summary>
    public required IReadOnlyList<IndexDefinitionRecord> Indexes { get; init; }

    /// <summary>
    /// Creates a new <see cref="TableDefinitionRecord"/> by parsing the data from the given <see cref="TpsRandomAccess"/> reader.
    /// </summary>
    public static TableDefinitionRecord Parse(TpsRandomAccess rx)
    {
        ArgumentNullException.ThrowIfNull(rx);

        short DriverVersion = rx.ReadShortLE();
        short RecordLength = rx.ReadShortLE();

        int fieldCount = rx.ReadShortLE();
        int memoCount = rx.ReadShortLE();
        int indexCount = rx.ReadShortLE();

        List<FieldDefinitionRecord> fields = [];
        List<MemoDefinitionRecord> memos = [];
        List<IndexDefinitionRecord> indexes = [];

        for (int i = 0; i < fieldCount; i++)
        {
            var fdr = FieldDefinitionRecord.Parse(rx);

            fields.Add(fdr);
        }
        for (int i = 0; i < memoCount; i++)
        {
            var mdr = MemoDefinitionRecord.Parse(rx);

            memos.Add(mdr);
        }
        for (int i = 0; i < indexCount; i++)
        {
            var idr = IndexDefinitionRecord.Parse(rx);

            indexes.Add(idr);
        }

        return new TableDefinitionRecord
        {
            DriverVersion = DriverVersion,
            RecordLength = RecordLength,
            Fields = fields.AsReadOnly(),
            Memos = memos.AsReadOnly(),
            Indexes = indexes.AsReadOnly()
        };
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"TableDefinition({DriverVersion},{RecordLength},{Fields.Count},{Memos.Count},{Indexes.Count}");

        foreach (var field in Fields)
        {
            sb.AppendLine($"  {field.ToString()}");
        }
        foreach (var memo in Memos)
        {
            sb.AppendLine($"  {memo.ToString()}");
        }
        foreach (var index in Indexes)
        {
            sb.AppendLine($"  {index.ToString()}");
        }

        sb.Append(")");

        return sb.ToString();
    }

    /// <summary>
    /// Gets a list of field values by parsing the given byte reader.
    /// </summary>
    /// <param name="rx"></param>
    /// <returns></returns>
    public IReadOnlyList<ITpsObject> ParseFields(TpsRandomAccess rx)
    {
        var values = new List<ITpsObject>(Fields.Count());

        foreach (var field in Fields)
        {
            if (field.IsArray)
            {
                int fieldSize = RecordLength / field.ElementCount;

                for (int i = 0; i < field.ElementCount; i++)
                {
                    values.Add(ParseField(field.Type, fieldSize, field, rx));
                }
            }
            else
            {
                values.Add(ParseField(field.Type, field.Length, field, rx));
            }
        }

        return values.AsReadOnly();
    }

    private ITpsObject ParseField(TpsTypeCode type, int length, FieldDefinitionRecord fieldDefinitionRecord, TpsRandomAccess rx)
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
            case TpsTypeCode.Byte:
                AssertEqual(1, length);
                return rx.ReadTpsByte();
            case TpsTypeCode.Short:
                AssertEqual(2, length);
                return rx.ReadTpsShort();
            case TpsTypeCode.UShort:
                AssertEqual(2, length);
                return rx.ReadTpsUnsignedShort();
            case TpsTypeCode.Date:
                return rx.ReadTpsDate();
            case TpsTypeCode.Time:
                return rx.ReadTpsTime();
            case TpsTypeCode.Long:
                AssertEqual(4, length);
                return rx.ReadTpsLong();
            case TpsTypeCode.ULong:
                AssertEqual(4, length);
                return rx.ReadTpsUnsignedLong();
            case TpsTypeCode.SReal:
                AssertEqual(4, length);
                return rx.ReadTpsFloat();
            case TpsTypeCode.Real:
                AssertEqual(8, length);
                return rx.ReadTpsDouble();
            case TpsTypeCode.Decimal:
                return rx.ReadTpsDecimal(length, fieldDefinitionRecord.BcdDigitsAfterDecimalPoint);
            case TpsTypeCode.String:
                return rx.ReadTpsString();
            case TpsTypeCode.CString:
                return rx.ReadTpsCString();
            case TpsTypeCode.PString:
                return rx.ReadTpsPString();
            case TpsTypeCode.Group:
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
