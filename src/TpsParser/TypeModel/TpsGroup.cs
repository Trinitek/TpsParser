using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TpsParser.Binary;
using TpsParser.Tps;

namespace TpsParser.TypeModel;

/// <summary>
/// Represents a compound data structure composed of one or more <see cref="IClaObject"/> instances.
/// </summary>
public sealed class TpsGroup : IComplex
{
    /// <inheritdoc/>
    public ClaTypeCode TypeCode => ClaTypeCode.Group;

    /// <summary>
    /// Gets the list of objects in this group.
    /// </summary>
    public IReadOnlyList<IClaObject> Objects { get; }

    /// <summary>
    /// Instantiates a new group that encapsulates the given values.
    /// </summary>
    public TpsGroup(IReadOnlyList<IClaObject> values)
    {
        Objects = values ?? throw new ArgumentNullException(nameof(values));
    }

    /// <summary>
    /// Builds a new <see cref="TpsGroup"/> and its child objects from the given binary reader and field definitions.
    /// </summary>
    /// <param name="rx">The binary reader.</param>
    /// <param name="encoding">The text encoding to use when parsing strings.</param>
    /// <param name="enumerator">An enumerator for a collection of field definitions, the first being the field to parse, followed by the remainder of the definitions.
    /// The enumerator must have already been advanced to the first item with a call to <see cref="IEnumerator.MoveNext"/>.</param>
    /// <returns></returns>
    internal static TpsGroup BuildFromFieldDefinitions(TpsRandomAccess rx, Encoding encoding, FieldDefinitionEnumerator enumerator)
    {
        ArgumentNullException.ThrowIfNull(rx);
        ArgumentNullException.ThrowIfNull(enumerator);

        var values = new List<IClaObject>();

        var groupDefinition = enumerator.Current;
        int expectedGroupLength = groupDefinition.Length / groupDefinition.ElementCount;
        int sumOfFollowingLengths = 0;

        while (enumerator.MoveNext())
        {
            sumOfFollowingLengths += enumerator.Current.Length;

            if (sumOfFollowingLengths > expectedGroupLength)
            {
                throw new TpsParserException($"The expected length of the group ({expectedGroupLength}) and the following fields ({sumOfFollowingLengths}) do not match.");
            }

            values.Add(ClaObject.ParseField(rx, encoding, enumerator));

            if (sumOfFollowingLengths == expectedGroupLength)
            {
                break;
            }
        }

        return new TpsGroup(values);
    }
}
