using System.Collections.Generic;
using TpsParser.TypeModel;

namespace TpsParser;

public sealed class FieldDefinitionEnumerable
{
    public static IEnumerable<IClaObject> EnumerateValues(IEnumerable<FieldDefinition> fieldDefinitions, DataRecordPayload dataPayload)
    {
        foreach (var fieldDefinition in fieldDefinitions)
        {
            ushort offset = fieldDefinition.Offset;

            var span = dataPayload.Content.Span[offset..];

            switch (fieldDefinition.TypeCode)
            {
                case FieldTypeCode.Byte:
                    yield return ClaBinaryPrimitives.ReadClaByte(span);
                    break;
                case FieldTypeCode.Short:
                    yield return ClaBinaryPrimitives.ReadClaShort(span);
                    break;
                case FieldTypeCode.UShort:
                    yield return ClaBinaryPrimitives.ReadClaUnsignedShort(span);
                    break;
                case FieldTypeCode.Long:
                    yield return ClaBinaryPrimitives.ReadClaLong(span);
                    break;
                case FieldTypeCode.ULong:
                    yield return ClaBinaryPrimitives.ReadClaUnsignedLong(span);
                    break;
                case FieldTypeCode.Date:
                    yield return ClaBinaryPrimitives.ReadClaDate(span);
                    break;
                case FieldTypeCode.Time:
                    yield return ClaBinaryPrimitives.ReadClaTime(span);
                    break;
                case FieldTypeCode.SReal:
                    yield return ClaBinaryPrimitives.ReadClaSingleReal(span);
                    break;
                case FieldTypeCode.Real:
                    yield return ClaBinaryPrimitives.ReadClaReal(span);
                    break;
                case FieldTypeCode.Decimal:
                    yield return ClaBinaryPrimitives.ReadClaDecimal(
                        span,
                        length: fieldDefinition.BcdElementLength,
                        digitsAfterDecimalPoint: fieldDefinition.BcdDigitsAfterDecimalPoint);
                    break;
                case FieldTypeCode.FString:
                    break;
                case FieldTypeCode.CString:
                    // TODO
                    break;
                case FieldTypeCode.PString:
                    // TODO
                    break;
                case FieldTypeCode.Group:
                    // TODO
                    break;
                case FieldTypeCode.None:
                default:
                    throw new TpsParserException($"Unknown field type code (0x{fieldDefinition.TypeCode}).");
            }
        }
    }
}
