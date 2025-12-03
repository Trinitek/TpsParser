using System;
using System.Collections.Generic;

namespace TpsParser.TypeModel;

/// <summary>
/// Represents a Clarion <c>GROUP</c>, which is a data structure containing one or more <see cref="IClaObject"/> instances.
/// </summary>
public readonly struct ClaGroup : IClaObject
{
    /// <summary>
    /// Gets the field iterator node that is used to materialize the values in this <c>GROUP</c>.
    /// </summary>
    public FieldIteratorNode FieldIteratorNode { get; }

    /// <summary>
    /// Gets the data record payload from which the <c>GROUP</c> values are materialized.
    /// </summary>
    public DataRecordPayload DataRecordPayload { get; }

    /// <inheritdoc/>
    public FieldTypeCode TypeCode => FieldTypeCode.Group;

    /// <summary>
    /// Gets the number of fields in this <c>GROUP</c>.
    /// </summary>
    public int Count => FieldIteratorNode.ChildIterators.Count;

    /// <summary>
    /// Instantiates a new <c>GROUP</c>.
    /// </summary>
    /// <param name="fieldIteratorNode">
    /// The field iterator node that is to be used to materialize the values in the <c>GROUP</c>.
    /// </param>
    /// <param name="dataRecordPayload">
    /// The data record payload from which the <c>GROUP</c> member values are materialized.
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    public ClaGroup(
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
        var results = FieldValueReader.EnumerateValues(FieldIteratorNode.ChildIterators, DataRecordPayload);

        foreach (var result in results)
        {
            yield return result;
        }
    }

    /// <summary>
    /// Gets a <see cref="FieldEnumerationResult"/> containing the field information and the <see cref="IClaObject"/>
    /// value at the given index.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public FieldEnumerationResult GetValue(ushort index)
    {
        var result = FieldValueReader.GetValue(FieldIteratorNode.ChildIterators[index], DataRecordPayload);

        return result;
    }

    /// <summary>
    /// Gets a <see cref="FieldEnumerationResult"/> containing the field information and the <see cref="IClaObject"/>
    /// value at the given index.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public FieldEnumerationResult this[ushort index] => GetValue(index);
}
