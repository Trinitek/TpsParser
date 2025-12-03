using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TpsParser.TypeModel;

namespace TpsParser;

/// <summary>
/// Represents field metadata and location information regarding how or where to read the field value
/// from a <see cref="DataRecordPayload"/>.
/// </summary>
public readonly record struct FieldDefinitionPointer
{
    /// <summary>
    /// Gets the field definition that backs this pointer.
    /// </summary>
    public required FieldDefinition Inner { get; init; }

    /// <inheritdoc cref="FieldDefinition.Name"/>
    public required string Name { get; init; }

    /// <inheritdoc cref="FieldDefinition.Offset"/>
    public required ushort Offset { get; init; }

    /// <inheritdoc cref="FieldDefinition.Length"/>
    public ushort Length => Inner.Length;

    /// <inheritdoc cref="FieldDefinition.ElementCount"/>
    public required ushort ElementCount { get; init; }

    /// <inheritdoc cref="FieldDefinition.TypeCode"/>
    public FieldTypeCode TypeCode => Inner.TypeCode;

    /// <inheritdoc cref="FieldDefinition.BcdElementLength"/>
    public byte BcdElementLength => Inner.BcdElementLength;

    /// <inheritdoc cref="FieldDefinition.BcdDigitsAfterDecimalPoint"/>
    public byte BcdDigitsAfterDecimalPoint => Inner.BcdDigitsAfterDecimalPoint;

    /// <inheritdoc cref="FieldDefinition.StringLength"/>
    public ushort StringLength => Inner.StringLength;

    /// <summary>
    /// Creates a new definition pointer from the given <see cref="FieldDefinition"/>.
    /// </summary>
    /// <param name="fieldDef"></param>
    /// <returns></returns>
    public static FieldDefinitionPointer Create(FieldDefinition fieldDef)
    {
        return new FieldDefinitionPointer
        {
            Inner = fieldDef,
            Name = fieldDef.Name,
            Offset = fieldDef.Offset,
            ElementCount = fieldDef.ElementCount,
        };
    }
}

/// <summary>
/// Encapsulates the field value and information about where the field data is located in a <see cref="DataRecordPayload"/>.
/// </summary>
/// <param name="FieldDefinition"></param>
/// <param name="Value"></param>
public readonly record struct FieldEnumerationResult(
    FieldDefinitionPointer FieldDefinition,
    IClaObject Value);

/// <summary>
/// Represents a node in a tree constructed from <see cref="FieldDefinition"/> items that reflects how various
/// scalar fields and <c>GROUP</c> composite data structures are laid out in a <see cref="DataRecordPayload"/>.
/// </summary>
/// <param name="DefinitionPointer">
/// A reference to a <see cref="FieldDefinition"/> that defines the type of field and where it is located in a data payload.
/// </param>
/// <param name="ChildIterators">
/// If <paramref name="DefinitionPointer"/> refers to a <c>GROUP</c>, contains the sub-fields that belong to this structure.
/// </param>
public readonly record struct FieldIteratorNode(
    FieldDefinitionPointer DefinitionPointer,
    List<FieldIteratorNode> ChildIterators);

/// <summary>
/// Contains methods to read field values from data records.
/// </summary>
public static class FieldValueReader
{
    /// <summary>
    /// Creates an array of nodes with which field values can be read from a data record.
    /// </summary>
    /// <param name="fieldDefinitions">An array of field definitions, i.e. from <see cref="TableDefinition.Fields"/>.</param>
    /// <param name="requestedFieldIndexes">
    /// A hash set of field indexes to read. If an index references a <c>GROUP</c> directly, all of the sub-fields within the
    /// <c>GROUP</c> are recursively added.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static ImmutableArray<FieldIteratorNode> CreateFieldIteratorNodes(
        ImmutableArray<FieldDefinition> fieldDefinitions,
        ImmutableHashSet<int> requestedFieldIndexes)
    {
        foreach (int fieldIndex in requestedFieldIndexes)
        {
            if (fieldIndex < 0 || fieldIndex >= fieldDefinitions.Length)
            {
                throw new ArgumentException($"Requested field index is out of range: {fieldIndex}");
            }
        }

        var orderedIndexes = requestedFieldIndexes.Order();

        List<FieldIteratorNode> iterators = [];

        foreach (int fieldIndex in orderedIndexes)
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

                    FieldIteratorNode pointer;

                    if (maybeExistingPointer is { } existing)
                    {
                        pointer = existing;
                    }
                    else
                    {
                        pointer = new FieldIteratorNode(
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
                var pointer = new FieldIteratorNode(
                    FieldDefinitionPointer.Create(fieldDef),
                    []);

                if (fieldDef.TypeCode == FieldTypeCode.Group)
                {
                    PopulateChildFieldsForGroup(
                        fieldDefinitions: fieldDefinitions,
                        group: pointer);
                }

                // Construct the linked-list of groups that need to be merged with the iterator list.

                FieldIteratorNode? outerGroupNode = null;

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

                    if (outerGroupNode == null)
                    {
                        // This is the first group; add the pointer here.

                        outerGroupNode = new(
                            FieldDefinitionPointer.Create(maybeGroup),
                            [pointer]);
                    }
                    else
                    {
                        // Successive groups are nested into each other.

                        var newGroupIterator = new FieldIteratorNode(
                            FieldDefinitionPointer.Create(maybeGroup),
                            [outerGroupNode.Value]);

                        outerGroupNode = newGroupIterator;
                    }
                }

                // Then merge that into the iterator list...

                if (outerGroupNode is null)
                {
                    // If no parent group was found, add the pointer directly to the list.

                    iterators.Add(pointer);
                }
                else
                {
                    MergeGroupNodes(
                        existingNodes: iterators,
                        groupToBeMerged: outerGroupNode.Value,
                        newNode: pointer);
                }
            }
        }

        return [.. iterators];
    }

    /// <summary>
    /// Recursively populates the child <see cref="FieldIteratorNode"/> elements of the given <c>GROUP</c> field.
    /// </summary>
    /// <param name="fieldDefinitions">An array of field definitions, i.e. from <see cref="TableDefinition.Fields"/>.</param>
    /// <param name="group">The <c>GROUP</c> node that is to be modified.</param>
    /// <returns>
    /// The <see cref="FieldDefinition.Index"/> of the next field outside of the <c>GROUP</c>
    /// field represented by <paramref name="group"/>.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="group"/> is not a <c>GROUP</c>.</exception>
    public static ushort PopulateChildFieldsForGroup(
        ImmutableArray<FieldDefinition> fieldDefinitions,
        FieldIteratorNode group)
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

            FieldIteratorNode childNode = new(
                FieldDefinitionPointer.Create(child),
                []);

            group.ChildIterators.Add(childNode);

            if (child.TypeCode == FieldTypeCode.Group)
            {
                // If the child is a group, recursively build the child fields for that.

                ushort nextIndex = PopulateChildFieldsForGroup(fieldDefinitions, group: childNode);

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
    /// Returns <see langword="true"/> if <paramref name="maybeGroup"/> is a <c>GROUP</c> and
    /// <paramref name="subject"/> is inside its scope, either as a direct parent-child or indirectly
    /// as a grandparent-grandchild.
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

    /// <summary>
    /// Merges a node describing one or more nested <c>GROUP</c> fields and their sub-fields into a list of existing nodes.
    /// </summary>
    /// <param name="existingNodes"></param>
    /// <param name="groupToBeMerged"></param>
    /// <param name="newNode">A reference to the last node in <paramref name="groupToBeMerged"/>.</param>
    public static void MergeGroupNodes(
        IList<FieldIteratorNode> existingNodes,
        FieldIteratorNode groupToBeMerged,
        FieldIteratorNode newNode)
    {
        // The expected shape of groupToBeMerged is essentially a linked list starting at the outer-most group
        // in the original FieldDefinition array, with exactly one ChildIterator that represents the next inner group,
        // and so on. The last inner group has one ChildIterator that is newNode.
        //
        // The newNode can either be a group itself (in the case where the user has SELECTed the inclusion
        // of the entire group by name) or it can be a simple scalar field like LONG or STRING (in the case where
        // the user has SELECTed the inclusion of a group sub-field).

        for (int ii = 0; ii < existingNodes.Count; ii++)
        {
            var existing = existingNodes[ii];

            if (existing.DefinitionPointer == newNode.DefinitionPointer)
            {
                // Found our end node.
                
                // We actually only need to swap the existing node with the new one if our target is itself a group,
                // but it doesn't matter if we do it for non-groups as well.
                //
                // If this is a group node, the new node will have been previously populated with all its child-fields
                // before merging, so this is important. Otherwise the result set would include the group itself but none
                // of its fields.
                existingNodes[ii] = newNode;

                // ...and finish.
                return;
            }
            else if (existing.DefinitionPointer == groupToBeMerged.DefinitionPointer)
            {
                // Otherwise, get the next child group and continue merging.

                var innerGroupToMergeOrNewPointer = groupToBeMerged.ChildIterators.Single();

                MergeGroupNodes(
                    existing.ChildIterators,
                    innerGroupToMergeOrNewPointer,
                    newNode);

                return;
            }
            else
            {
                continue;
            }
        }

        // Not found in list; add it.

        existingNodes.Add(groupToBeMerged);

        return;
    }

    /// <summary>
    /// Recursively flattens a hierarchy of <see cref="FieldIteratorNode"/> elements, producing a sequence of leaf nodes with fully
    /// qualified names.
    /// </summary>
    /// <remarks>
    /// Array and group nodes are traversed recursively. For array nodes, each element is expanded
    /// with its index in the name. Group nodes are expanded with their child nodes. Only leaf nodes (fields that are
    /// not groups or arrays) are returned in the sequence. This method does not modify the original nodes; it returns
    /// new node instances with adjusted names.
    /// </remarks>
    /// <param name="nodes">
    /// The collection of root field iterator nodes to flatten. Each node may represent a field, group, or array structure.
    /// </param>
    /// <param name="namePrefix">
    /// The prefix to prepend to each node's name in the resulting sequence. Typically used to build fully qualified
    /// field names.
    /// </param>
    /// <returns>
    /// An enumerable sequence of leaf-level <see cref="FieldIteratorNode"/> instances, each with its name adjusted to reflect its
    /// position in the hierarchy. The sequence is empty if no nodes are provided.
    /// </returns>
    public static IEnumerable<FieldIteratorNode> RecursivelyFlattenNodes(
        IEnumerable<FieldIteratorNode> nodes,
        string? namePrefix = null)
    {
        namePrefix ??= string.Empty;

        foreach (var node in nodes)
        {
            // Special case for arrays

            if (node.DefinitionPointer.ElementCount > 1)
            {
                for (ushort elementIndex = 0; elementIndex < node.DefinitionPointer.ElementCount; elementIndex++)
                {
                    var elementNode = GetNodeForArrayIndex(node, elementIndex);

                    string nodeName = $"{namePrefix}{elementNode.DefinitionPointer.Name}[{elementIndex}]";

                    var adjustedPointer = elementNode.DefinitionPointer with { Name = nodeName };

                    var adjustedElementNode = elementNode with { DefinitionPointer = adjustedPointer };

                    if (adjustedElementNode.ChildIterators.Count > 0)
                    {
                        foreach (var child in RecursivelyFlattenNodes(adjustedElementNode.ChildIterators, $"{nodeName}."))
                        {
                            yield return child;
                        }
                    }
                    else
                    {
                        yield return adjustedElementNode;
                    }
                }

                // Stop processing the array node.
                continue;
            }

            if (node.ChildIterators.Count > 0)
            {
                string groupNodeName = $"{namePrefix}{node.DefinitionPointer.Name}";

                foreach (var child in RecursivelyFlattenNodes(node.ChildIterators, $"{groupNodeName}."))
                {
                    string nodeName = child.DefinitionPointer.Name;

                    var adjustedPointer = child.DefinitionPointer with { Name = nodeName };

                    var adjustedNode = child with { DefinitionPointer = adjustedPointer };

                    yield return adjustedNode;
                }
            }
            
            else if (node.DefinitionPointer.TypeCode != FieldTypeCode.Group)
            {
                string nodeName = $"{namePrefix}{node.DefinitionPointer.Name}";

                var adjustedPointer = node.DefinitionPointer with { Name = nodeName };

                var adjustedNode = node with { DefinitionPointer = adjustedPointer };

                yield return adjustedNode;
            }
        }
    }

    /// <summary>
    /// Reads the given data record payload and returns a field value for the given array node.
    /// </summary>
    /// <param name="arrayPointer"></param>
    /// <param name="dataRecordPayload"></param>
    /// <returns></returns>
    public static IEnumerable<FieldEnumerationResult> EnumerateValuesForArray(
        FieldIteratorNode arrayPointer,
        DataRecordPayload dataRecordPayload)
    {
        ushort elementCount = arrayPointer.DefinitionPointer.ElementCount;

        for (ushort elementIndex = 0; elementIndex < elementCount; elementIndex++)
        {
            var newPointer = GetNodeForArrayIndex(arrayPointer, elementIndex);

            yield return GetValue(newPointer, dataRecordPayload);
        }
    }

    /// <summary>
    /// Creates a node instance that points to a single value in an array.
    /// </summary>
    /// <param name="arrayPointer">The array node.</param>
    /// <param name="elementIndex">The zero-based index into the array.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static FieldIteratorNode GetNodeForArrayIndex(FieldIteratorNode arrayPointer, ushort elementIndex)
    {
        ushort elementCount = arrayPointer.DefinitionPointer.ElementCount;

        if (elementIndex >= elementCount)
        {
            throw new ArgumentOutOfRangeException($"Element index ({elementIndex}) exceeds the number of elements in pointer ({elementCount}).");
        }

        // Create a new single-element iterator with a fixed-up offset

        ushort baseOffset = arrayPointer.DefinitionPointer.Offset;
        ushort lengthPerElement = (ushort)(arrayPointer.DefinitionPointer.Length / elementCount);

        ushort adjustedOffset = (ushort)(baseOffset + (lengthPerElement * elementIndex));

        var newDefPtr = arrayPointer.DefinitionPointer with { Offset = adjustedOffset, ElementCount = 1 };

        var newIterator = arrayPointer with { DefinitionPointer = newDefPtr };

        return newIterator;
    }

    /// <summary>
    /// Reads the given data record payload and returns a field value for the given node.
    /// </summary>
    /// <param name="fieldIteratorNode"></param>
    /// <param name="dataRecordPayload"></param>
    /// <returns></returns>
    /// <exception cref="TpsParserException"></exception>
    public static FieldEnumerationResult GetValue(
        FieldIteratorNode fieldIteratorNode,
        DataRecordPayload dataRecordPayload)
    {
        var fieldDefPointer = fieldIteratorNode.DefinitionPointer;

        // Special case: Array of any type.

        if (fieldDefPointer.ElementCount > 1)
        {
            var array = new ClaArray(
                fieldIteratorNode: fieldIteratorNode,
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
                return new(fieldDefPointer, new ClaGroup(fieldIteratorNode, dataRecordPayload));
            case FieldTypeCode.None:
            default:
                throw new TpsParserException($"Unknown field type code (0x{fieldDefPointer.TypeCode}).");
        }
    }

    /// <summary>
    /// Reads the given data record payload and returns field values for the given nodes.
    /// </summary>
    /// <param name="fieldIteratorNodes"></param>
    /// <param name="dataRecordPayload"></param>
    /// <returns></returns>
    public static IEnumerable<FieldEnumerationResult> EnumerateValues(
        IEnumerable<FieldIteratorNode> fieldIteratorNodes,
        DataRecordPayload dataRecordPayload)
    {
        foreach (var node in fieldIteratorNodes)
        {
            yield return GetValue(node, dataRecordPayload);
        }
    }
}
