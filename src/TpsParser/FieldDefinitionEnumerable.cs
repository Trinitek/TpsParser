using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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

public readonly record struct FieldIteratorPointer(
    FieldDefinitionPointer DefinitionPointer,
    List<FieldIteratorPointer> ChildIterators);

public sealed class FieldDefinitionEnumerable
{
    public static ImmutableArray<FieldIteratorPointer> CreateFieldIterator(ImmutableArray<FieldDefinition> fieldDefinitions, ImmutableHashSet<int> requestedFieldIndices)
    {
        foreach (int fieldIndex in requestedFieldIndices)
        {
            if (fieldIndex < 0 || fieldIndex >= fieldDefinitions.Length)
            {
                throw new ArgumentException($"Requested field index is out of range: {fieldIndex}");
            }
        }

        var orderedIndices = requestedFieldIndices.Order();

        List<FieldIteratorPointer> iterators = [];

        foreach (int fieldIndex in orderedIndices)
        {
            var fieldDef = fieldDefinitions[fieldIndex];

            // For the selected field, we need to walk the field definitions to determine if it's inside one or more nested groups,
            // and then recursively add those groups to our iterator list.
            //
            // If an index points directly to a group and not a sub-field in a group, then that group and all its sub-fields are
            // added, and as before we recursively add the parent groups that it belongs to, if any.

            if (fieldIndex == 0)
            {
                // This is the first index; this field is not in a group. Add it directly.

                if (fieldDef.TypeCode == FieldTypeCode.Group)
                {
                    var maybeExistingPointer = iterators.FirstOrDefault(fp => fp.DefinitionPointer.Inner == fieldDef);

                    FieldIteratorPointer pointer;

                    if (maybeExistingPointer is { } existing)
                    {
                        pointer = existing;
                    }
                    else
                    {
                        pointer = new FieldIteratorPointer(
                            FieldDefinitionPointer.Create(fieldDef),
                            []);

                        iterators.Add(pointer);
                    }

                    PopulateChildFieldsForGroup(
                        fieldDefinitions: fieldDefinitions,
                        group: pointer);
                }
                else
                {
                    iterators.Add(new(
                        FieldDefinitionPointer.Create(fieldDef),
                        []));
                }
            }
            else
            {
                var pointer = new FieldIteratorPointer(
                    FieldDefinitionPointer.Create(fieldDef),
                    []);

                if (fieldDef.TypeCode == FieldTypeCode.Group)
                {
                    PopulateChildFieldsForGroup(
                        fieldDefinitions: fieldDefinitions,
                        group: pointer);
                }

                // Construct the linked-list of groups that need to be merged with the iterator list.

                FieldIteratorPointer? outerGroupIterator = null;

                for (int fi = fieldIndex - 1; fi >= 0; fi--)
                {
                    var maybeGroup = fieldDefinitions[fi];

                    bool isInsideGroup = IsFieldInsideGroup(
                        maybeGroup: maybeGroup,
                        subject: fieldDef);

                    if (isInsideGroup is false)
                    {
                        continue;
                    }

                    if (outerGroupIterator == null)
                    {
                        // This is the first group; add the pointer here.

                        outerGroupIterator = new(
                            FieldDefinitionPointer.Create(maybeGroup),
                            [pointer]);
                    }
                    else
                    {
                        // Successive groups are nested into each other.

                        var newGroupIterator = new FieldIteratorPointer(
                            FieldDefinitionPointer.Create(maybeGroup),
                            [outerGroupIterator.Value]);

                        outerGroupIterator = newGroupIterator;
                    }
                }

                // Then merge that into the iterator list...

                if (outerGroupIterator is null)
                {
                    // If no parent group was found, add the pointer directly to the list.

                    iterators.Add(pointer);
                }
                else
                {
                    MergeGroupPointers(
                        existingIterators: iterators,
                        groupToBeMerged: outerGroupIterator.Value,
                        newPointer: pointer);
                }
            }
        }

        return [.. iterators];
    }

    /// <summary>
    /// Recursively populates the child <see cref="FieldIteratorPointer"/> elements of the given group field and returns the
    /// <see cref="FieldDefinition.Index"/> of the next non-child field outside of the group.
    /// </summary>
    /// <param name="fieldDefinitions"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static ushort PopulateChildFieldsForGroup(ImmutableArray<FieldDefinition> fieldDefinitions, FieldIteratorPointer group)
    {
        var groupDefPtr = group.DefinitionPointer;

        if (groupDefPtr.TypeCode != FieldTypeCode.Group)
        {
            throw new ArgumentException($"Cannot populate child fields for a field that is not a GROUP. Field index ({groupDefPtr.Inner.Index}) of type ({groupDefPtr.TypeCode}).", nameof(group));
        }

        ushort firstChildIndex = (ushort)(groupDefPtr.Inner.Index + 1);
        ushort i = firstChildIndex;

        while (i < fieldDefinitions.Length)
        {
            var child = fieldDefinitions[i];

            bool childIsInsideGroup = IsFieldInsideGroup(
                maybeGroup: groupDefPtr.Inner,
                subject: child);

            if (childIsInsideGroup is false)
            {
                break;
            }

            FieldIteratorPointer childPtr = new(
                FieldDefinitionPointer.Create(child),
                []);

            group.ChildIterators.Add(childPtr);

            if (child.TypeCode == FieldTypeCode.Group)
            {
                // If the child is a group, recursively build the child fields for that.

                ushort nextIndex = PopulateChildFieldsForGroup(fieldDefinitions, group: childPtr);

                i = nextIndex;
            }
            else
            {
                i++;
            }
        }

        return i;
    }

    /// <summary>
    /// Returns <see langword="true"/> if <paramref name="maybeGroup"/> is a group and <paramref name="subject"/>
    /// is inside its scope, either as a direct parent-child or indirectly as a grandparent-grandchild.
    /// </summary>
    /// <param name="maybeGroup"></param>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static bool IsFieldInsideGroup(FieldDefinition maybeGroup, FieldDefinition subject)
    {
        if (maybeGroup is not { TypeCode: FieldTypeCode.Group } group)
        {
            return false;
        }

        // A group is not inside of itself,
        // nor can it be inside of a child group that has the same offset.
        if (subject.Index <= group.Index)
        {
            return false;
        }

        // Compensate for arrays-of-groups.
        ushort groupElementLength = (ushort)(group.Length / group.ElementCount);

        return (group.Offset <= subject.Offset)
            && ((subject.Offset + subject.Length) <= (group.Offset + groupElementLength));
    }

    public static void MergeGroupPointers(
        IList<FieldIteratorPointer> existingIterators,
        FieldIteratorPointer groupToBeMerged,
        FieldIteratorPointer newPointer)
    {
        // The expected shape of groupToBeMerged is essentially a linked list starting at the outer-most group
        // in the original FieldDefinition array, with exactly one ChildIterator that represents the next inner group,
        // and so on. The last inner group has one ChildIterator that is newPointer.
        //
        // The newPointer can either be a group itself (in the case where the user has SELECTed the inclusion
        // of the entire group by name) or it can be a simple scalar field like LONG or STRING (in the case where
        // the user has SELECTed the inclusion of a group sub-field).

        for (int ii = 0; ii < existingIterators.Count; ii++)
        {
            var existing = existingIterators[ii];

            if (existing.DefinitionPointer == newPointer.DefinitionPointer)
            {
                // Found our end pointer.
                
                // We actually only need to swap the existing pointer with the new one if our target is itself a group,
                // but it doesn't matter if we do it for non-groups as well.
                //
                // If this is a group pointer, the new pointer will have been previously populated with all its child-fields
                // before merging, so this is important. Otherwise the result set would include the group itself but none
                // of its fields.
                existingIterators[ii] = newPointer;

                // ...and finish.
                return;
            }
            else if (existing.DefinitionPointer == groupToBeMerged.DefinitionPointer)
            {
                // Otherwise, get the next child group and continue merging.

                var innerGroupToMergeOrNewPointer = groupToBeMerged.ChildIterators.Single();

                MergeGroupPointers(
                    existing.ChildIterators,
                    innerGroupToMergeOrNewPointer,
                    newPointer);

                return;
            }
            else
            {
                continue;
            }
        }

        // Not found in list; add it.

        existingIterators.Add(groupToBeMerged);

        return;
    }

    public static IEnumerable<FieldEnumerationResult> EnumerateValuesForArray(
        FieldIteratorPointer arrayPointer,
        DataRecordPayload dataRecordPayload)
    {
        ushort elementCount = arrayPointer.DefinitionPointer.ElementCount;

        for (int elementIndex = 0; elementIndex < elementCount; elementIndex++)
        {
            // Create a new single-element iterator with a fixed-up offset

            ushort baseOffset = arrayPointer.DefinitionPointer.Offset;
            ushort lengthPerElement = (ushort)(arrayPointer.DefinitionPointer.Length / elementCount);

            ushort adjustedOffset = (ushort)(baseOffset + (lengthPerElement * elementIndex));

            var newDefPtr = arrayPointer.DefinitionPointer with { Offset = adjustedOffset, ElementCount = 1 };

            var newIterator = arrayPointer with { DefinitionPointer = newDefPtr };

            yield return GetValue(newIterator, dataRecordPayload);
        }
    }

    public static FieldEnumerationResult GetValue(
        FieldIteratorPointer fieldIteratorPointer,
        DataRecordPayload dataRecordPayload)
    {
        var fieldDefPointer = fieldIteratorPointer.DefinitionPointer;

        // Special case: Array of any type.

        if (fieldDefPointer.ElementCount > 1)
        {
            var array = new ClaArray(
                fieldIteratorPointer: fieldIteratorPointer,
                dataRecordPayload: dataRecordPayload);

            return new(
                FieldDefinition: fieldDefPointer,
                Value: array);
        }

        // Remainder of cases proceed as normal.

        ushort offset = fieldDefPointer.Offset;

        var span = dataRecordPayload.Content.Span[offset..];

        switch (fieldDefPointer.TypeCode)
        {
            case FieldTypeCode.Byte:
                return new(fieldDefPointer, ClaBinaryPrimitives.ReadClaByte(span));
            case FieldTypeCode.Short:
                return new(fieldDefPointer, ClaBinaryPrimitives.ReadClaShort(span));
            case FieldTypeCode.UShort:
                return new(fieldDefPointer, ClaBinaryPrimitives.ReadClaUnsignedShort(span));
            case FieldTypeCode.Long:
                return new(fieldDefPointer, ClaBinaryPrimitives.ReadClaLong(span));
            case FieldTypeCode.ULong:
                return new(fieldDefPointer, ClaBinaryPrimitives.ReadClaUnsignedLong(span));
            case FieldTypeCode.Date:
                return new(fieldDefPointer, ClaBinaryPrimitives.ReadClaDate(span));
            case FieldTypeCode.Time:
                return new(fieldDefPointer, ClaBinaryPrimitives.ReadClaTime(span));
            case FieldTypeCode.SReal:
                return new(fieldDefPointer, ClaBinaryPrimitives.ReadClaSingleReal(span));
            case FieldTypeCode.Real:
                return new(fieldDefPointer, ClaBinaryPrimitives.ReadClaReal(span));
            case FieldTypeCode.Decimal:
                return new(fieldDefPointer, ClaBinaryPrimitives.ReadClaDecimal(
                    span,
                    length: fieldDefPointer.BcdElementLength,
                    digitsAfterDecimalPoint: fieldDefPointer.BcdDigitsAfterDecimalPoint));
            case FieldTypeCode.FString:
                return new(fieldDefPointer, new ClaFString(dataRecordPayload.Content[offset..(offset + fieldDefPointer.StringLength)]));
            case FieldTypeCode.CString:
                return new(fieldDefPointer, new ClaCString(dataRecordPayload.Content[offset..(offset + fieldDefPointer.StringLength)]));
            case FieldTypeCode.PString:
                return new(fieldDefPointer, new ClaPString(dataRecordPayload.Content[offset..(offset + fieldDefPointer.StringLength)]));
            case FieldTypeCode.Group:
                return new(fieldDefPointer, new ClaGroup(fieldIteratorPointer, dataRecordPayload));
            case FieldTypeCode.None:
            default:
                throw new TpsParserException($"Unknown field type code (0x{fieldDefPointer.TypeCode}).");
        }
    }

    public static IEnumerable<FieldEnumerationResult> EnumerateValues(
        IEnumerable<FieldIteratorPointer> fieldIteratorPointers,
        DataRecordPayload dataRecordPayload)
    {
        foreach (var fieldIteratorPointer in fieldIteratorPointers)
        {
            yield return GetValue(fieldIteratorPointer, dataRecordPayload);
        }
    }
}
