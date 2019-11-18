using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TpsParser.Binary;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents an array of objects.
    /// </summary>
    public sealed class TpsArray<TTpsObject, TImplType> :
        TpsObject<IReadOnlyList<TTpsObject>>,
        IConvertible<IEnumerable<TTpsObject>>, IConvertible<IReadOnlyList<TTpsObject>>,
        IHasConverterExtension
        where TTpsObject : TpsObject
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

            Value = items.Cast<TTpsObject>().ToList().AsReadOnly();
        }

        internal TpsArray(IReadOnlyList<TTpsObject> items)
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
        internal override bool AsBoolean() => Value.Any();

        IEnumerable<TTpsObject> IConvertible<IEnumerable<TTpsObject>>.AsType() => Value;

        IReadOnlyList<TTpsObject> IConvertible<IReadOnlyList<TTpsObject>>.AsType() => Value;

        /// <inheritdoc/>
        public IConverterExtension ConverterExtension
        {
            get
            {
                if (_converterExtension is null)
                {
                    _converterExtension = new TpsArrayConverterExtension<TImplType>(
                        enumerableConverter: () => (IEnumerable<TImplType>)Value.Select(v => v.Value),
                        readOnlyListConverter: () => Value.Select(v => (TImplType)v.Value).ToList());
                }

                return _converterExtension;
            }
        }
        private IConverterExtension _converterExtension;
    }

    internal sealed class TpsArrayConverterExtension<TImplType> : IConverterExtension, IConvertible<IEnumerable<TImplType>>, IConvertible<IReadOnlyList<TImplType>>
    {
        internal TpsArrayConverterExtension(
            Func<IEnumerable<TImplType>> enumerableConverter,
            Func<IReadOnlyList<TImplType>> readOnlyListConverter)
        {
            EnumerableConverter = enumerableConverter ?? throw new ArgumentNullException(nameof(enumerableConverter));
            ReadOnlyListConverter = readOnlyListConverter ?? throw new ArgumentNullException(nameof(readOnlyListConverter));
        }

        private Func<IEnumerable<TImplType>> EnumerableConverter { get; }
        private Func<IReadOnlyList<TImplType>> ReadOnlyListConverter { get; }

        IEnumerable<TImplType> IConvertible<IEnumerable<TImplType>>.AsType() => EnumerableConverter.Invoke();

        IReadOnlyList<TImplType> IConvertible<IReadOnlyList<TImplType>>.AsType() => ReadOnlyListConverter.Invoke();
    }

    internal static class TpsArrayExtensions
    {
        private static TpsObject Create(TpsTypeCode typeCode, IReadOnlyList<TpsObject> items)
        {
            switch (typeCode)
            {
                case TpsTypeCode.Byte:
                    return new TpsArray<TpsByte, byte>((IReadOnlyList<TpsByte>)items);
                case TpsTypeCode.Short:
                    return new TpsArray<TpsShort, short>((IReadOnlyList<TpsShort>)items);
                case TpsTypeCode.UShort:
                    return new TpsArray<TpsUnsignedShort, ushort>((IReadOnlyList<TpsUnsignedShort>)items);
                case TpsTypeCode.Date:
                    return new TpsArray<TpsDate, DateTime?>((IReadOnlyList<TpsDate>)items);
                case TpsTypeCode.Time:
                    return new TpsArray<TpsTime, TimeSpan>((IReadOnlyList<TpsTime>)items);
                case TpsTypeCode.Long:
                    return new TpsArray<TpsLong, int>((IReadOnlyList<TpsLong>)items);
                case TpsTypeCode.ULong:
                    return new TpsArray<TpsUnsignedLong, uint>((IReadOnlyList<TpsUnsignedLong>)items);
                case TpsTypeCode.SReal:
                    return new TpsArray<TpsFloat, float>((IReadOnlyList<TpsFloat>)items);
                case TpsTypeCode.Real:
                    return new TpsArray<TpsDouble, double>((IReadOnlyList<TpsDouble>)items);
                case TpsTypeCode.Decimal:
                    return new TpsArray<TpsDecimal, string>((IReadOnlyList<TpsDecimal>)items);
                case TpsTypeCode.String:
                    return new TpsArray<TpsString, string>((IReadOnlyList<TpsString>)items);
                case TpsTypeCode.CString:
                    return new TpsArray<TpsCString, string>((IReadOnlyList<TpsCString>)items);
                case TpsTypeCode.PString:
                    return new TpsArray<TpsPString, string>((IReadOnlyList<TpsPString>)items);
                case TpsTypeCode.Group:
                    return new TpsArray<TpsGroup, IReadOnlyList<TpsObject>>((IReadOnlyList<TpsGroup>)items);
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

        internal static TpsObject Parse(RandomAccess rx, Encoding encoding, FieldDefinitionEnumerator enumerator)
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
                arrayValues[i] = TpsObject.ParseNonArrayField(rx, encoding, fieldSize, enumerator);
            }

            return Create(current.Type, arrayValues);
        }
    }
}
