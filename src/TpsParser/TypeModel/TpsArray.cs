using System;
using System.Collections.Generic;
using System.Text;

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
    public FieldTypeCode TypeCode { get; }

    /// <inheritdoc/>
    public IReadOnlyList<T> Objects => _objects;
    private readonly IReadOnlyList<T> _objects;

    IReadOnlyList<IClaObject> ITpsArray.Objects => (IReadOnlyList<IClaObject>)_objects;

    /// <summary>
    /// Instantiates a new array.
    /// </summary>
    /// <param name="typeCode"></param>
    /// <param name="items"></param>
    public TpsArray(FieldTypeCode typeCode, IReadOnlyList<T> items)
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
    private static ITpsArray Create(FieldTypeCode typeCode, IReadOnlyList<IClaObject> items)
    {
        switch (typeCode)
        {
            case FieldTypeCode.Byte:
                return new TpsArray<ClaByte>(typeCode, (IReadOnlyList<ClaByte>)items);
            case FieldTypeCode.Short:
                return new TpsArray<ClaShort>(typeCode, (IReadOnlyList<ClaShort>)items);
            case FieldTypeCode.UShort:
                return new TpsArray<ClaUnsignedShort>(typeCode, (IReadOnlyList<ClaUnsignedShort>)items);
            case FieldTypeCode.Date:
                return new TpsArray<ClaDate>(typeCode, (IReadOnlyList<ClaDate>)items);
            case FieldTypeCode.Time:
                return new TpsArray<ClaTime>(typeCode, (IReadOnlyList<ClaTime>)items);
            case FieldTypeCode.Long:
                return new TpsArray<ClaLong>(typeCode, (IReadOnlyList<ClaLong>)items);
            case FieldTypeCode.ULong:
                return new TpsArray<ClaUnsignedLong>(typeCode, (IReadOnlyList<ClaUnsignedLong>)items);
            case FieldTypeCode.SReal:
                return new TpsArray<ClaSingleReal>(typeCode, (IReadOnlyList<ClaSingleReal>)items);
            case FieldTypeCode.Real:
                return new TpsArray<ClaReal>(typeCode, (IReadOnlyList<ClaReal>)items);
            case FieldTypeCode.Decimal:
                return new TpsArray<ClaDecimal>(typeCode, (IReadOnlyList<ClaDecimal>)items);
            case FieldTypeCode.FString:
                return new TpsArray<ClaFString>(typeCode, (IReadOnlyList<ClaFString>)items);
            case FieldTypeCode.CString:
                return new TpsArray<ClaCString>(typeCode, (IReadOnlyList<ClaCString>)items);
            case FieldTypeCode.PString:
                return new TpsArray<ClaPString>(typeCode, (IReadOnlyList<ClaPString>)items);
            case FieldTypeCode.Group:
                return new TpsArray<TpsGroup>(typeCode, (IReadOnlyList<TpsGroup>)items);
            default:
                throw new ArgumentException($"Arrays of type '{typeCode}' are not supported.", nameof(typeCode));
        }
    }

    private static IList<IClaObject> CreateArray(FieldTypeCode typeCode, int size)
    {
        IList<IClaObject> CreateArrayCore<T>() where T : IClaObject, new()
        {
            return (IList<IClaObject>)new List<T>(capacity: size);
        }

        switch (typeCode)
        {
            case FieldTypeCode.Byte:
                return CreateArrayCore<ClaByte>();
            case FieldTypeCode.Short:
                return CreateArrayCore<ClaShort>();
            case FieldTypeCode.UShort:
                return CreateArrayCore<ClaUnsignedShort>();
            case FieldTypeCode.Date:
                return CreateArrayCore<ClaDate>();
            case FieldTypeCode.Time:
                return CreateArrayCore<ClaTime>();
            case FieldTypeCode.Long:
                return CreateArrayCore<ClaLong>();
            case FieldTypeCode.ULong:
                return CreateArrayCore<ClaUnsignedLong>();
            case FieldTypeCode.SReal:
                return CreateArrayCore<ClaSingleReal>();
            case FieldTypeCode.Real:
                return CreateArrayCore<ClaReal>();
            case FieldTypeCode.Decimal:
                return CreateArrayCore<ClaDecimal>();
            case FieldTypeCode.FString:
                return CreateArrayCore<ClaFString>();
            case FieldTypeCode.CString:
                return CreateArrayCore<ClaCString>();
            case FieldTypeCode.PString:
                return CreateArrayCore<ClaPString>();
            case FieldTypeCode.Group:
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
        var arrayValues = CreateArray(current.TypeCode, current.ElementCount);

        // Very important for GROUP arrays! Clusters of fields are repeated, so we need to reset our field definition position for each group item.
        int nextEnumeratorPosition = enumerator.Position;

        for (int i = 0; i < current.ElementCount; i++)
        {
            enumerator.Position = nextEnumeratorPosition;
            arrayValues[i] = ClaObject.ParseScalarField(rx, encoding, fieldSize, enumerator);
        }

        return Create(current.TypeCode, arrayValues.AsReadOnly());
    }
}
