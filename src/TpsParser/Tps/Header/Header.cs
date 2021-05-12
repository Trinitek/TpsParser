using System;

namespace TpsParser.Tps.Header
{
    /// <summary>
    /// Encapsulates information that describes a record.
    /// </summary>
    public interface IHeader
    {
        /// <summary>
        /// Gets the table number to which the header belongs.
        /// </summary>
        int TableNumber { get; }

        /// <summary>
        /// Gets the type code that represents the header.
        /// </summary>
        HeaderKind Kind { get; }
    }

    /// <inheritdoc/>
    public abstract class HeaderBase : IHeader
    {
        /// <inheritdoc/>
        public int TableNumber { get; }
        
        /// <inheritdoc/>
        public HeaderKind Kind { get; }

        /// <summary>
        /// Instantiates a new header.
        /// </summary>
        /// <param name="tableNumber"></param>
        /// <param name="kind"></param>
        public HeaderBase(int tableNumber, HeaderKind kind)
            : base()
        {
            TableNumber = tableNumber;
            Kind = kind;
        }

        /// <summary>
        /// Throws an exception if the table type is not equal to the type given.
        /// </summary>
        /// <param name="expected"></param>
        protected void AssertIsType(HeaderKind expected)
        {
            if (Kind != expected)
            {
                throw new ArgumentException($"Header is not of expected type. Expected {expected} but was {Kind}.");
            }
        }
    }
}
