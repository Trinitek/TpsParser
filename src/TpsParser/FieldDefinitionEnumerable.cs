using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using TpsParser.TypeModel;

namespace TpsParser;

public readonly record struct FieldDefinitionPointer
{
    public required FieldDefinition Inner { get; init; }
    public required ushort Offset { get; init; }
    public required ushort Length { get; init; }
    public required ushort ElementCount { get; init; }
    public required FieldTypeCode TypeCode { get; init; }
    public byte BcdElementLength => Inner.BcdElementLength;
    public byte BcdDigitsAfterDecimalPoint => Inner.BcdDigitsAfterDecimalPoint;
    public ushort StringLength => Inner.StringLength;

    public static FieldDefinitionPointer Create(FieldDefinition fieldDef)
    {
        return new FieldDefinitionPointer
        {
            Inner = fieldDef,
            Offset = fieldDef.Offset,
            Length = fieldDef.Length,
            ElementCount = fieldDef.ElementCount,
            TypeCode = fieldDef.TypeCode,
        };
    }

    public static FieldDefinitionPointer CreateArrayElement(FieldDefinitionPointer fieldDef, ushort index)
    {
        ushort lengthPerElement = (ushort)(fieldDef.Length / fieldDef.ElementCount);
        ushort offset = (ushort)(fieldDef.Offset + (index * lengthPerElement));

        return new FieldDefinitionPointer
        {
            Inner = fieldDef.Inner,
            Offset = offset,
            Length = lengthPerElement,
            ElementCount = 1,
            TypeCode = fieldDef.TypeCode,
        };
    }
}

public readonly record struct FieldEnumerationResult(
    FieldDefinitionPointer FieldDefinition,
    IClaObject Value);

public sealed class FieldDefinitionEnumerable
{
    public static IEnumerable<FieldDefinitionPointer> CreateFieldDefSlims(ImmutableArray<FieldDefinition> fieldDefinitions)
    {
        // We need to turn our flat list of field definitions into nested pointers (FieldDefinitionSlim instances)
        // so that we can recursively iterate them. Step zero is to get an IEnumerable of all the level-0 fields,
        // that is, all of the fields that are _not_ already nested in a group. Or more literally, if we encounter
        // a GROUP field, we return a FieldDefinitionSlim for that, then skip all its inner fields, then return the next.

        for (int i = 0; i < fieldDefinitions.Length; i++)
        {
            var fieldDefinition = fieldDefinitions[i];

            if (fieldDefinition.TypeCode == FieldTypeCode.Group)
            {
                yield return FieldDefinitionPointer.Create(fieldDefinition);

                ushort groupLength = fieldDefinition.Length;
                ushort sum = 0;

                while (true)
                {
                    i++;
                    
                    sum += fieldDefinitions[i].Length;
                    
                    if (sum == groupLength)
                    {
                        break;
                    }

                    if (sum > groupLength)
                    {
                        throw new TpsParserException($"Sum of GROUP sub-field lengths ({sum}) does not match expected GROUP length ({groupLength}) on GROUP {fieldDefinition.FullName}.");
                    }
                }

                continue;
            }
            else
            {
                yield return FieldDefinitionPointer.Create(fieldDefinition);
            }
        }
    }

    public static IEnumerable<FieldEnumerationResult> EnumerateValues(IEnumerable<FieldDefinitionPointer> fieldDefinitions, DataRecordPayload dataPayload)
    {
        foreach (var fieldDefinition in fieldDefinitions)
        {
            // Special case 1: Array.

            if (fieldDefinition.ElementCount > 1)
            {
                IEnumerable<FieldDefinitionPointer> EnumerateArrayElement()
                {
                    ushort lengthPerElement = (ushort)(fieldDefinition.Length / fieldDefinition.ElementCount);

                    for (ushort i = 0; i < fieldDefinition.ElementCount; i++)
                    {
                        yield return FieldDefinitionPointer.CreateArrayElement(fieldDefinition, i);
                    }
                }

                foreach (var inner in EnumerateValues(EnumerateArrayElement(), dataPayload))
                {
                    yield return inner;
                }

                // Stop processing this field.
                continue;
            }

            // Special case 2: Group.

            if (fieldDefinition.TypeCode == FieldTypeCode.Group)
            {

                // TODO...

                // Stop processing this field.
                continue;
            }

            // Remainder of cases proceed as normal.

            ushort offset = fieldDefinition.Offset;

            var span = dataPayload.Content.Span[offset..];

            switch (fieldDefinition.TypeCode)
            {
                case FieldTypeCode.Byte:
                    yield return new(fieldDefinition, ClaBinaryPrimitives.ReadClaByte(span));
                    break;
                case FieldTypeCode.Short:
                    yield return new(fieldDefinition, ClaBinaryPrimitives.ReadClaShort(span));
                    break;
                case FieldTypeCode.UShort:
                    yield return new(fieldDefinition, ClaBinaryPrimitives.ReadClaUnsignedShort(span));
                    break;
                case FieldTypeCode.Long:
                    yield return new(fieldDefinition, ClaBinaryPrimitives.ReadClaLong(span));
                    break;
                case FieldTypeCode.ULong:
                    yield return new(fieldDefinition, ClaBinaryPrimitives.ReadClaUnsignedLong(span));
                    break;
                case FieldTypeCode.Date:
                    yield return new(fieldDefinition, ClaBinaryPrimitives.ReadClaDate(span));
                    break;
                case FieldTypeCode.Time:
                    yield return new(fieldDefinition, ClaBinaryPrimitives.ReadClaTime(span));
                    break;
                case FieldTypeCode.SReal:
                    yield return new(fieldDefinition, ClaBinaryPrimitives.ReadClaSingleReal(span));
                    break;
                case FieldTypeCode.Real:
                    yield return new(fieldDefinition, ClaBinaryPrimitives.ReadClaReal(span));
                    break;
                case FieldTypeCode.Decimal:
                    yield return new(fieldDefinition, ClaBinaryPrimitives.ReadClaDecimal(
                        span,
                        length: fieldDefinition.BcdElementLength,
                        digitsAfterDecimalPoint: fieldDefinition.BcdDigitsAfterDecimalPoint));
                    break;
                case FieldTypeCode.FString:
                    yield return new(fieldDefinition, new ClaFString(dataPayload.Content[offset..fieldDefinition.StringLength]));
                    break;
                case FieldTypeCode.CString:
                    yield return new(fieldDefinition, new ClaCString(dataPayload.Content[offset..fieldDefinition.StringLength]));
                    break;
                case FieldTypeCode.PString:
                    yield return new(fieldDefinition, new ClaPString(dataPayload.Content[offset..fieldDefinition.StringLength]));
                    break;
                case FieldTypeCode.Group:
                    // GROUPs should be handled as a special case above.
                    throw new TpsParserException("GROUP field not handled.");
                case FieldTypeCode.None:
                default:
                    throw new TpsParserException($"Unknown field type code (0x{fieldDefinition.TypeCode}).");
            }
        }
    }
}
