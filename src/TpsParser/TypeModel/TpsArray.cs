using System;
using System.Collections.Generic;
using System.Text;
using TpsParser.Binary;
using TpsParser.Tps;

namespace TpsParser.TypeModel;

/// <summary>
/// Represents an array of objects.
/// </summary>
public interface ITpsArray : IClaObject
{
    // /// <inheritdoc/>
    //TpsTypeCode TypeCode { get; }

    /// <summary>
    /// Gets the collection of objects in this array.
    /// </summary>
    IReadOnlyList<IClaObject> Objects { get; }
}

/// <summary>
/// Represents an array of objects.
/// </summary>
public sealed class TpsArray<T> : IComplex, ITpsArray
    where T : IClaObject
{
    /// Gets the type code of the objects in this array.
    public ClaTypeCode TypeCode { get; }

    /// <inheritdoc/>
    public IReadOnlyList<T> Objects => _objects;
    private readonly IReadOnlyList<T> _objects;

    IReadOnlyList<IClaObject> ITpsArray.Objects => (IReadOnlyList<IClaObject>)_objects;

    /// <summary>
    /// Instantiates a new array.
    /// </summary>
    /// <param name="typeCode"></param>
    /// <param name="items"></param>
    public TpsArray(ClaTypeCode typeCode, IReadOnlyList<T> items)
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
    private static ITpsArray Create(ClaTypeCode typeCode, IReadOnlyList<IClaObject> items)
    {
        switch (typeCode)
        {
            case ClaTypeCode.Byte:
                return new TpsArray<ClaByte>(typeCode, (IReadOnlyList<ClaByte>)items);
            case ClaTypeCode.Short:
                return new TpsArray<ClaShort>(typeCode, (IReadOnlyList<ClaShort>)items);
            case ClaTypeCode.UShort:
                return new TpsArray<ClaUnsignedShort>(typeCode, (IReadOnlyList<ClaUnsignedShort>)items);
            case ClaTypeCode.Date:
                return new TpsArray<ClaDate>(typeCode, (IReadOnlyList<ClaDate>)items);
            case ClaTypeCode.Time:
                return new TpsArray<ClaTime>(typeCode, (IReadOnlyList<ClaTime>)items);
            case ClaTypeCode.Long:
                return new TpsArray<ClaLong>(typeCode, (IReadOnlyList<ClaLong>)items);
            case ClaTypeCode.ULong:
                return new TpsArray<ClaUnsignedLong>(typeCode, (IReadOnlyList<ClaUnsignedLong>)items);
            case ClaTypeCode.SReal:
                return new TpsArray<ClaSingleReal>(typeCode, (IReadOnlyList<ClaSingleReal>)items);
            case ClaTypeCode.Real:
                return new TpsArray<ClaReal>(typeCode, (IReadOnlyList<ClaReal>)items);
            case ClaTypeCode.Decimal:
                return new TpsArray<ClaDecimal>(typeCode, (IReadOnlyList<ClaDecimal>)items);
            case ClaTypeCode.FString:
                return new TpsArray<ClaFString>(typeCode, (IReadOnlyList<ClaFString>)items);
            case ClaTypeCode.CString:
                return new TpsArray<ClaCString>(typeCode, (IReadOnlyList<ClaCString>)items);
            case ClaTypeCode.PString:
                return new TpsArray<ClaPString>(typeCode, (IReadOnlyList<ClaPString>)items);
            case ClaTypeCode.Group:
                return new TpsArray<TpsGroup>(typeCode, (IReadOnlyList<TpsGroup>)items);
            default:
                throw new ArgumentException($"Arrays of type '{typeCode}' are not supported.", nameof(typeCode));
        }
    }

    private static IList<IClaObject> CreateArray(ClaTypeCode typeCode, int size)
    {
        IList<IClaObject> CreateArrayCore<T>() where T : IClaObject, new()
        {
            return (IList<IClaObject>)new List<T>(capacity: size);
        }

        switch (typeCode)
        {
            case ClaTypeCode.Byte:
                return CreateArrayCore<ClaByte>();
            case ClaTypeCode.Short:
                return CreateArrayCore<ClaShort>();
            case ClaTypeCode.UShort:
                return CreateArrayCore<ClaUnsignedShort>();
            case ClaTypeCode.Date:
                return CreateArrayCore<ClaDate>();
            case ClaTypeCode.Time:
                return CreateArrayCore<ClaTime>();
            case ClaTypeCode.Long:
                return CreateArrayCore<ClaLong>();
            case ClaTypeCode.ULong:
                return CreateArrayCore<ClaUnsignedLong>();
            case ClaTypeCode.SReal:
                return CreateArrayCore<ClaSingleReal>();
            case ClaTypeCode.Real:
                return CreateArrayCore<ClaReal>();
            case ClaTypeCode.Decimal:
                return CreateArrayCore<ClaDecimal>();
            case ClaTypeCode.FString:
                return CreateArrayCore<ClaFString>();
            case ClaTypeCode.CString:
                return CreateArrayCore<ClaCString>();
            case ClaTypeCode.PString:
                return CreateArrayCore<ClaPString>();
            case ClaTypeCode.Group:
            default:
                throw new ArgumentException($"Arrays of type '{typeCode}' are not supported.", nameof(typeCode));
        }
    }

    internal static IClaObject Parse(TpsRandomAccess rx, Encoding encoding, FieldDefinitionEnumerator enumerator)
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
            arrayValues[i] = ClaObject.ParseScalarField(rx, encoding, fieldSize, enumerator);
        }

        return Create(current.Type, arrayValues.AsReadOnly());
    }
}
