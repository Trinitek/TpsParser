using System;
using System.Collections.Generic;
using System.Text;
using TpsParser.Binary;
using TpsParser.Tps;

namespace TpsParser.TypeModel;

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
    /// Gets the type code of the objects in this array.
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

    private static IList<ITpsObject> CreateArray(TpsTypeCode typeCode, int size)
    {
        IList<ITpsObject> CreateArrayCore<T>() where T : ITpsObject, new()
        {
            return (IList<ITpsObject>)new List<T>(capacity: size);
        }

        switch (typeCode)
        {
            case TpsTypeCode.Byte:
                return CreateArrayCore<TpsByte>();
            case TpsTypeCode.Short:
                return CreateArrayCore<TpsShort>();
            case TpsTypeCode.UShort:
                return CreateArrayCore<TpsUnsignedShort>();
            case TpsTypeCode.Date:
                return CreateArrayCore<TpsDate>();
            case TpsTypeCode.Time:
                return CreateArrayCore<TpsTime>();
            case TpsTypeCode.Long:
                return CreateArrayCore<TpsLong>();
            case TpsTypeCode.ULong:
                return CreateArrayCore<TpsUnsignedLong>();
            case TpsTypeCode.SReal:
                return CreateArrayCore<TpsFloat>();
            case TpsTypeCode.Real:
                return CreateArrayCore<TpsDouble>();
            case TpsTypeCode.Decimal:
                return CreateArrayCore<TpsDecimal>();
            case TpsTypeCode.String:
                return CreateArrayCore<TpsString>();
            case TpsTypeCode.CString:
                return CreateArrayCore<TpsCString>();
            case TpsTypeCode.PString:
                return CreateArrayCore<TpsPString>();
            case TpsTypeCode.Group:
            default:
                throw new ArgumentException($"Arrays of type '{typeCode}' are not supported.", nameof(typeCode));
        }
    }

    internal static ITpsObject Parse(TpsRandomAccess rx, Encoding encoding, FieldDefinitionEnumerator enumerator)
    {
        ArgumentNullException.ThrowIfNull(rx);
        ArgumentNullException.ThrowIfNull(encoding);
        ArgumentNullException.ThrowIfNull(enumerator);

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

        return Create(current.Type, arrayValues.AsReadOnly());
    }
}
