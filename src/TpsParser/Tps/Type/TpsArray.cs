using System;
using System.Collections.Generic;
using System.Text;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents an array of objects.
    /// </summary>
    public interface ITpsArray : ITpsObject
    {
        // /// <inheritdoc/>
        //TpsTypeCode TypeCode { get; }

        /// <summary>
        /// Gets the collection of objects in this array.
        /// </summary>
        IReadOnlyList<ITpsObject> Objects { get; }
    }

    /// <summary>
    /// Represents an array of objects.
    /// </summary>
    public sealed class TpsArray<T> : IComplex, ITpsArray
        where T : ITpsObject
    {
        /// <inheritdoc/>
        public TpsTypeCode TypeCode { get; }

        /// <inheritdoc/>
        public IReadOnlyList<T> Objects => _objects;
        private readonly IReadOnlyList<T> _objects;

        IReadOnlyList<ITpsObject> ITpsArray.Objects => (IReadOnlyList<ITpsObject>)_objects;

        /// <summary>
        /// Instantiates a new array.
        /// </summary>
        /// <param name="typeCode"></param>
        /// <param name="items"></param>
        public TpsArray(TpsTypeCode typeCode, IReadOnlyList<T> items)
        {
            TypeCode = typeCode;
            _objects = items ?? throw new ArgumentNullException(nameof(items));
        }

        /// <summary>
        /// Gets the number of elements in the array.
        /// </summary>
        public int Count => Objects.Count;
    }

    internal static class TpsArrayExtensions
    {
        private static ITpsArray Create(TpsTypeCode typeCode, IReadOnlyList<ITpsObject> items)
        {
            switch (typeCode)
            {
                case TpsTypeCode.Byte:
                    return new TpsArray<TpsByte>(typeCode, (IReadOnlyList<TpsByte>)items);
                case TpsTypeCode.Short:
                    return new TpsArray<TpsShort>(typeCode, (IReadOnlyList<TpsShort>)items);
                case TpsTypeCode.UShort:
                    return new TpsArray<TpsUnsignedShort>(typeCode, (IReadOnlyList<TpsUnsignedShort>)items);
                case TpsTypeCode.Date:
                    return new TpsArray<TpsDate>(typeCode, (IReadOnlyList<TpsDate>)items);
                case TpsTypeCode.Time:
                    return new TpsArray<TpsTime>(typeCode, (IReadOnlyList<TpsTime>)items);
                case TpsTypeCode.Long:
                    return new TpsArray<TpsLong>(typeCode, (IReadOnlyList<TpsLong>)items);
                case TpsTypeCode.ULong:
                    return new TpsArray<TpsUnsignedLong>(typeCode, (IReadOnlyList<TpsUnsignedLong>)items);
                case TpsTypeCode.SReal:
                    return new TpsArray<TpsFloat>(typeCode, (IReadOnlyList<TpsFloat>)items);
                case TpsTypeCode.Real:
                    return new TpsArray<TpsDouble>(typeCode, (IReadOnlyList<TpsDouble>)items);
                case TpsTypeCode.Decimal:
                    return new TpsArray<TpsDecimal>(typeCode, (IReadOnlyList<TpsDecimal>)items);
                case TpsTypeCode.String:
                    return new TpsArray<TpsString>(typeCode, (IReadOnlyList<TpsString>)items);
                case TpsTypeCode.CString:
                    return new TpsArray<TpsCString>(typeCode, (IReadOnlyList<TpsCString>)items);
                case TpsTypeCode.PString:
                    return new TpsArray<TpsPString>(typeCode, (IReadOnlyList<TpsPString>)items);
                case TpsTypeCode.Group:
                    return new TpsArray<TpsGroup>(typeCode, (IReadOnlyList<TpsGroup>)items);
                default:
                    throw new ArgumentException($"Arrays of type '{typeCode}' are not supported.", nameof(typeCode));
            }
        }

        private static ITpsObject[] CreateArray(TpsTypeCode typeCode, int size)
        {
            return null;

            //switch (typeCode)
            //{
            //    case TpsTypeCode.Byte:
            //        return new TpsByte[size];
            //    case TpsTypeCode.Short:
            //        return new TpsShort[size];
            //    case TpsTypeCode.UShort:
            //        return new TpsUnsignedShort[size];
            //    case TpsTypeCode.Date:
            //        return new TpsDate[size];
            //    case TpsTypeCode.Time:
            //        return new TpsTime[size];
            //    case TpsTypeCode.Long:
            //        return new TpsLong[size];
            //    case TpsTypeCode.ULong:
            //        return new TpsUnsignedLong[size];
            //    case TpsTypeCode.SReal:
            //        return new TpsFloat[size];
            //    case TpsTypeCode.Real:
            //        return new TpsDouble[size];
            //    case TpsTypeCode.Decimal:
            //        return new TpsDecimal[size];
            //    case TpsTypeCode.String:
            //        return new TpsString[size];
            //    case TpsTypeCode.CString:
            //        return new TpsCString[size];
            //    case TpsTypeCode.PString:
            //        return new TpsPString[size];
            //    case TpsTypeCode.Group:
            //        return new TpsGroup[size];
            //    default:
            //        throw new ArgumentException($"Arrays of type '{typeCode}' are not supported.", nameof(typeCode));
            //}
        }

        internal static ITpsObject Parse(TpsReader rx, Encoding encoding, FieldDefinitionEnumerator enumerator)
        {
            if (rx is null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            if (encoding is null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            if (enumerator is null)
            {
                throw new ArgumentNullException(nameof(enumerator));
            }

            var current = enumerator.Current ?? throw new ArgumentException("The first item in the enumerator is null.", nameof(enumerator));

            if (!current.IsArray)
            {
                throw new ArgumentException("The first item in the enumerator must be an array.", nameof(enumerator));
            }

            int fieldSize = current.Length / current.ElementCount;
            var arrayValues = CreateArray(current.Type, current.ElementCount);

            // Very important for GROUP arrays! Clusters of fields are repeated, so we need to reset our field definition position for each group item.
            int nextEnumeratorPosition = enumerator.Position;

            for (int i = 0; i < current.ElementCount; i++)
            {
                enumerator.Position = nextEnumeratorPosition;
                arrayValues[i] = TpsObject.ParseScalarField(rx, encoding, fieldSize, enumerator);
            }

            return Create(current.Type, arrayValues);
        }
    }
}
