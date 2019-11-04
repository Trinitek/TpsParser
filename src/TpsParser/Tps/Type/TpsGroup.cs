using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TpsParser.Binary;
using TpsParser.Tps.Record;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a grouping of fields.
    /// </summary>
    public sealed class TpsGroup : TpsObject<IReadOnlyList<TpsObject>>
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.Group;

        /// <summary>
        /// Instantiates a new group that encapsulates the given values.
        /// </summary>
        public TpsGroup(IReadOnlyList<TpsObject> values)
        {
            Value = values ?? throw new ArgumentNullException(nameof(values));
        }

        /// <summary>
        /// Builds a new <see cref="TpsGroup"/> and its child objects from the given binary reader and field definitions.
        /// </summary>
        /// <param name="rx">The binary reader.</param>
        /// <param name="encoding">The text encoding to use when parsing strings.</param>
        /// <param name="fieldDefinitionRecords">A collection of field definitions, the first being the field to parse, followed by the remainder of the definitions.</param>
        /// <returns></returns>
        public static TpsGroup BuildFromFieldDefinitions(RandomAccess rx, Encoding encoding, IEnumerable<IFieldDefinitionRecord> fieldDefinitionRecords)
        {
            if (rx is null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            if (fieldDefinitionRecords is null)
            {
                throw new ArgumentNullException(nameof(fieldDefinitionRecords));
            }

            var values = new List<TpsObject>();

            int expectedGroupLength = fieldDefinitionRecords.First().Length;
            int sumOfFollowingLengths = 0;

            for (int fieldIndex = 1; fieldIndex < fieldDefinitionRecords.Count(); fieldIndex++)
            {
                var field = fieldDefinitionRecords.Skip(fieldIndex).First();
                var remainingFieldDefinitions = fieldDefinitionRecords.Skip(2);

                sumOfFollowingLengths += field.Length;
                
                if (sumOfFollowingLengths > expectedGroupLength)
                {
                    throw new TpsParserException($"The expected length of the group ({expectedGroupLength}) and the following fields ({sumOfFollowingLengths}) do not match.");
                }

                values.Add(ParseField(rx, encoding, field.Length, remainingFieldDefinitions));

                if (sumOfFollowingLengths == expectedGroupLength)
                {
                    break;
                }
            }

            return new TpsGroup(values);
        }

        /// <summary>
        /// Returns true if the data size is not zero.
        /// </summary>
        internal override bool AsBoolean() => Value.Any(v => v.AsBoolean());
    }
}
