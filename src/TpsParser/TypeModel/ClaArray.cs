using System;
using System.Collections.Generic;

namespace TpsParser.TypeModel;

/// <summary>
/// <para>
/// Represents an array of <see cref="IClaObject"/> instances.
/// </para>
/// <para>
/// A Clarion array type does not have its own <see cref="FieldTypeCode"/> but instead inherits the code
/// of the objects within it. Objects in the array will be of a single type.
/// </para>
/// </summary>
public readonly struct ClaArray : IClaObject
{
    /// <summary>
    /// Gets the field iterator node that is used to materialize the values in this array.
    /// </summary>
    public FieldIteratorNode FieldIteratorNode { get; }

    /// <summary>
    /// Gets the data record payload from which the array values are materialized.
    /// </summary>
    public DataRecordPayload DataRecordPayload { get; }

    /// <summary>
    /// Gets the type code of the object contained in this array.
    /// </summary>
    public FieldTypeCode TypeCode => FieldIteratorNode.DefinitionPointer.TypeCode;

    /// <summary>
    /// Gets the number of elements in this array.
    /// </summary>
    public int Count => FieldIteratorNode.DefinitionPointer.ElementCount;

    /// <summary>
    /// Instantiates a new array.
    /// </summary>
    /// <param name="fieldIteratorNode">
    /// The field iterator node that is to be used to materialize the values in the array.
    /// </param>
    /// <param name="dataRecordPayload">
    /// The data record payload from which the array values are materialized.
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    public ClaArray(
        FieldIteratorNode fieldIteratorNode,
        DataRecordPayload dataRecordPayload)
    {
        FieldIteratorNode = fieldIteratorNode;
        DataRecordPayload = dataRecordPayload;
    }

    /// <summary>
    /// Gets an enumerable of <see cref="FieldEnumerationResult"/> values containing the field information
    /// and the <see cref="IClaObject"/> value.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<FieldEnumerationResult> GetValues()
    {
        return FieldValueReader.EnumerateValuesForArray(FieldIteratorNode, DataRecordPayload);
    }

    /// <summary>
    /// Gets a <see cref="FieldEnumerationResult"/> containing the field information and the <see cref="IClaObject"/>
    /// value at the given index.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public FieldEnumerationResult GetValue(ushort index)
    {
        var pointer = FieldValueReader.GetNodeForArrayIndex(FieldIteratorNode, index);

        return FieldValueReader.GetValue(pointer, DataRecordPayload);
    }

    /// <summary>
    /// Gets a <see cref="FieldEnumerationResult"/> containing the field information and the <see cref="IClaObject"/>
    /// value at the given index.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public FieldEnumerationResult this[ushort index] => GetValue(index);
}
