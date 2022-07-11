using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents an array of objects.
    /// </summary>
    public interface ITpsArray
    {
        /// <summary>
        /// Gets the collection of objects in this array.
        /// </summary>
        IReadOnlyList<TpsObject> Objects { get; }
    }

    /// <summary>
    /// Represents an array of objects.
    /// </summary>
    public sealed class TpsArray<TTpsObject> :
        TpsObject<IReadOnlyList<TTpsObject>>, ITpsArray
        where TTpsObject : TpsObject
    {
        /// <inheritdoc/>
        public override TpsTypeCode TypeCode => TpsTypeCode.None;

        IReadOnlyList<TpsObject> ITpsArray.Objects => Value;

        /// <summary>
        /// Instantiates a new array.
        /// </summary>
        /// <param name="items"></param>
        public TpsArray(IReadOnlyList<TTpsObject> items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            Value = items;
        }

        /// <summary>
        /// Gets the number of elements in the array.
        /// </summary>
        public int Count => Value.Count;

        /// <inheritdoc/>
        public override Maybe<bool> ToBoolean() => new Maybe<bool>(Value.Any());

        /// <inheritdoc/>
        public override Maybe<IReadOnlyList<TpsObject>> ToArray() => new Maybe<IReadOnlyList<TpsObject>>(Value);
    }

    internal static class TpsArrayExtensions
    {
        private static TpsObject Create(TpsTypeCode typeCode, IReadOnlyList<TpsObject> items)
        {
            switch (typeCode)
            {
                case TpsTypeCode.Byte:
                    return new TpsArray<TpsByte>((IReadOnlyList<TpsByte>)items);
                case TpsTypeCode.Short:
                    return new TpsArray<TpsShort>((IReadOnlyList<TpsShort>)items);
                case TpsTypeCode.UShort:
                    return new TpsArray<TpsUnsignedShort>((IReadOnlyList<TpsUnsignedShort>)items);
                case TpsTypeCode.Date:
                    return new TpsArray<TpsDate>((IReadOnlyList<TpsDate>)items);
                case TpsTypeCode.Time:
                    return new TpsArray<TpsTime>((IReadOnlyList<TpsTime>)items);
                case TpsTypeCode.Long:
                    return new TpsArray<TpsLong>((IReadOnlyList<TpsLong>)items);
                case TpsTypeCode.ULong:
                    return new TpsArray<TpsUnsignedLong>((IReadOnlyList<TpsUnsignedLong>)items);
                case TpsTypeCode.SReal:
                    return new TpsArray<TpsFloat>((IReadOnlyList<TpsFloat>)items);
                case TpsTypeCode.Real:
                    return new TpsArray<TpsDouble>((IReadOnlyList<TpsDouble>)items);
                case TpsTypeCode.Decimal:
                    return new TpsArray<TpsDecimal>((IReadOnlyList<TpsDecimal>)items);
                case TpsTypeCode.String:
                    return new TpsArray<TpsString>((IReadOnlyList<TpsString>)items);
                case TpsTypeCode.CString:
                    return new TpsArray<TpsCString>((IReadOnlyList<TpsCString>)items);
                case TpsTypeCode.PString:
                    return new TpsArray<TpsPString>((IReadOnlyList<TpsPString>)items);
                case TpsTypeCode.Group:
                    return new TpsArray<TpsGroup>((IReadOnlyList<TpsGroup>)items);
                default:
                    throw new ArgumentException($"Arrays of type '{typeCode}' are not supported.", nameof(typeCode));
            }
        }

        private static TpsObject[] CreateArray(TpsTypeCode typeCode, int size)
        {
            switch (typeCode)
            {
                case TpsTypeCode.Byte:
                    return new TpsByte[size];
                case TpsTypeCode.Short:
                    return new TpsShort[size];
                case TpsTypeCode.UShort:
                    return new TpsUnsignedShort[size];
                case TpsTypeCode.Date:
                    return new TpsDate[size];
                case TpsTypeCode.Time:
                    return new TpsTime[size];
                case TpsTypeCode.Long:
                    return new TpsLong[size];
                case TpsTypeCode.ULong:
                    return new TpsUnsignedLong[size];
                case TpsTypeCode.SReal:
                    return new TpsFloat[size];
                case TpsTypeCode.Real:
                    return new TpsDouble[size];
                case TpsTypeCode.Decimal:
                    return new TpsDecimal[size];
                case TpsTypeCode.String:
                    return new TpsString[size];
                case TpsTypeCode.CString:
                    return new TpsCString[size];
                case TpsTypeCode.PString:
                    return new TpsPString[size];
                case TpsTypeCode.Group:
                    return new TpsGroup[size];
                default:
                    throw new ArgumentException($"Arrays of type '{typeCode}' are not supported.", nameof(typeCode));
            }
        }

        internal static TpsObject Parse(TpsReader rx, Encoding encoding, FieldDefinitionEnumerator enumerator)
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
