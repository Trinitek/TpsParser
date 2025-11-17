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

public sealed record FieldIteratorPointer(
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

                    bool isInsideGroup = FieldIsInsideGroup(
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
                            [outerGroupIterator]);

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
                        groupToBeMerged: outerGroupIterator,
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
        ArgumentNullException.ThrowIfNull(group);

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

            bool childIsInsideGroup = FieldIsInsideGroup(
                maybeGroup: groupDefPtr.Inner,
                subject: child);

            if (childIsInsideGroup is false)
            {
                break;
            }

            FieldIteratorPointer childPtr = new(FieldDefinitionPointer.Create(child), []);

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
    public static bool FieldIsInsideGroup(FieldDefinition maybeGroup, FieldDefinition subject)
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

    public static IEnumerable<FieldDefinitionPointer> GetLevel0Pointers(ImmutableArray<FieldDefinition> fieldDefinitions)
    {
        // We need to turn our flat list of field definitions into nested pointers (FieldDefinitionPointer instances)
        // so that we can recursively iterate them. Step zero is to get an IEnumerable of all the level-0 fields,
        // that is, all of the fields that are _not_ already nested in a group. Or more literally, if we encounter
        // a GROUP field, we return a FieldDefinitionPoint for that, then skip all its inner fields, then return the next.

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

    //public static IEnumerable<FieldDefinition> GetFieldsInGroup()

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

                yield return new(
                    fieldDefinition,
                    new ClaArray(EnumerateArrayElement(), dataPayload));

                // Stop processing this field.
                continue;
            }

            // Special case 2: Group.

            if (fieldDefinition.TypeCode == FieldTypeCode.Group)
            {
                IEnumerable<FieldDefinitionPointer> EnumerateGroupElement()
                {
                    ushort groupLength = fieldDefinition.Length;
                    ushort sum = 0;

                    while (true)
                    {

                    }

                    // TODO...
                }

                yield return new(
                    fieldDefinition,
                    new ClaGroup(EnumerateGroupElement(), dataPayload));

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
