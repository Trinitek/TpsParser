using System;
using System.Collections.Generic;
using System.Linq;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents an array of objects.
    /// </summary>
    public sealed class TpsArray : TpsObject<IReadOnlyList<TpsObject>>
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.None;

        /// <summary>
        /// Instantiates a new array of the given objects.
        /// </summary>
        /// <param name="items"></param>
        public TpsArray(IEnumerable<TpsObject> items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            Value = items.ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets the number of elements in the array.
        /// </summary>
        public int Count => Value.Count;

        /// <inheritdoc/>
        protected override bool AsBoolean() => Value.Any();
    }
}
