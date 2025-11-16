using System;
using System.Collections.Generic;
using System.Linq;

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
public sealed class ClaArray : IClaObject
{
    /// <summary>
    /// Gets the enumerable of field definition pointers that are used to materialize the values in this array.
    /// </summary>
    public IEnumerable<FieldDefinitionPointer> FieldDefinitions { get; }

    /// <summary>
    /// Gets the data record payload from which the array values are materialized.
    /// </summary>
    public DataRecordPayload DataRecordPayload { get; }

    /// <summary>
    /// Gets the type code of the object contained in this array.
    /// </summary>
    public FieldTypeCode TypeCode => FieldDefinitions.First().TypeCode;

    /// <summary>
    /// Instantiates a new array.
    /// </summary>
    /// <param name="fieldDefinitions">
    /// The field definition pointers that are to be used to materialize the values in the array.
    /// The values are presumed to have the same <see cref="FieldDefinitionPointer.TypeCode"/> value.
    /// If the types are not the same, enumerating the values may result in undesirable behavior.
    /// </param>
    /// <param name="dataRecordPayload">
    /// The data record payload from which the array values are materialized.
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    public ClaArray(
        IEnumerable<FieldDefinitionPointer> fieldDefinitions,
        DataRecordPayload dataRecordPayload)
    {
        FieldDefinitions = fieldDefinitions ?? throw new ArgumentNullException(nameof(fieldDefinitions));
        DataRecordPayload = dataRecordPayload;
    }

    /// <summary>
    /// Gets an enumerable of <see cref="FieldEnumerationResult"/> values containing the field information
    /// and the <see cref="IClaObject"/> value.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<FieldEnumerationResult> GetValues()
    {
        var results = FieldDefinitionEnumerable.EnumerateValues(FieldDefinitions, DataRecordPayload);

        foreach (var result in results)
        {
            yield return result;
        }
    }
}

/// <summary>
/// Represents a Clarion <c>GROUP</c>, which is a data structure containing one or more <see cref="IClaObject"/> instances.
/// </summary>
public sealed class ClaGroup : IClaObject
{
    /// <summary>
    /// Gets the enumerable of field definition pointers that are used to materialize the values in this <c>GROUP</c>.
    /// </summary>
    public IEnumerable<FieldDefinitionPointer> FieldDefinitions { get; }

    /// <summary>
    /// Gets the data record payload from which the <c>GROUP</c> values are materialized.
    /// </summary>
    public DataRecordPayload DataRecordPayload { get; }

    /// <inheritdoc/>
    public FieldTypeCode TypeCode => FieldTypeCode.Group;

    /// <summary>
    /// Instantiates a new <c>GROUP</c>.
    /// </summary>
    /// <param name="fieldDefinitions">
    /// The field definition pointers that are to be used to materialize the values in the <c>GROUP</c>.
    /// </param>
    /// <param name="dataRecordPayload">
    /// The data record payload from which the <c>GROUP</c> member values are materialized.
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    public ClaGroup(
        IEnumerable<FieldDefinitionPointer> fieldDefinitions,
        DataRecordPayload dataRecordPayload)
    {
        FieldDefinitions = fieldDefinitions ?? throw new ArgumentNullException(nameof(fieldDefinitions));
        DataRecordPayload = dataRecordPayload;
    }

    /// <summary>
    /// Gets an enumerable of <see cref="FieldEnumerationResult"/> values containing the field information
    /// and the <see cref="IClaObject"/> value.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<FieldEnumerationResult> GetValues()
    {
        var results = FieldDefinitionEnumerable.EnumerateValues(FieldDefinitions, DataRecordPayload);

        foreach (var result in results)
        {
            yield return result;
        }
    }
}
