using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a typed object within the TopSpeed file.
    /// </summary>
    public interface ITpsObject
    {
        /// <summary>
        /// Gets the type code of the object.
        /// </summary>
        TpsTypeCode TypeCode { get; }

        /// <summary>
        /// Gets a <see cref="bool"/> representation of the value as governed by Clarion logic evaluation rules for the type.
        /// </summary>
        /// <returns></returns>
        Maybe<bool> ToBoolean();

        /// <summary>
        /// Gets an <see cref="sbyte"/> representation of the value.
        /// </summary>
        /// <returns></returns>
        Maybe<sbyte> ToSByte();
        
        /// <summary>
        /// Gets a <see cref="byte"/> representation of the value.
        /// </summary>
        /// <returns></returns>
        Maybe<byte> ToByte();

        /// <summary>
        /// Gets a <see cref="short"/> representation of the value.
        /// </summary>
        /// <returns></returns>
        Maybe<short> ToInt16();

        /// <summary>
        /// Gets a <see cref="uint"/> representation of the value.
        /// </summary>
        /// <returns></returns>
        Maybe<ushort> ToUInt16();

        /// <summary>
        /// Gets an <see cref="int"/> representation of the value.
        /// </summary>
        /// <returns></returns>
        Maybe<int> ToInt32();

        /// <summary>
        /// Gets a <see cref="uint"/> representation of the value.
        /// </summary>
        /// <returns></returns>
        Maybe<uint> ToUInt32();

        /// <summary>
        /// Gets a <see cref="long"/> representation of the value.
        /// </summary>
        /// <returns></returns>
        Maybe<long> ToInt64();

        /// <summary>
        /// Gets a <see cref="ulong"/> representation of the value.
        /// </summary>
        /// <returns></returns>
        Maybe<ulong> ToUInt64();

        /// <summary>
        /// Gets a <see cref="float"/> representation of the value.
        /// </summary>
        /// <returns></returns>
        Maybe<float> ToFloat();

        /// <summary>
        /// Gets a <see cref="double"/> representation of the value.
        /// </summary>
        /// <returns></returns>
        Maybe<double> ToDouble();

        /// <summary>
        /// Gets a <see cref="decimal"/> representation of the value.
        /// </summary>
        /// <returns></returns>
        Maybe<decimal> ToDecimal();

        /// <summary>
        /// Gets a <see cref="DateTime"/> representation of the value.
        /// </summary>
        /// <returns></returns>
        Maybe<DateTime?> ToDateTime();

        /// <summary>
        /// Gets a <see cref="TimeSpan"/> representation of the value.
        /// </summary>
        /// <returns></returns>
        Maybe<TimeSpan> ToTimeSpan();

        /// <summary>
        /// Gets an array representation of the value.
        /// </summary>
        /// <returns></returns>
        Maybe<IReadOnlyList<ITpsObject>> ToArray();

        /// <summary>
        /// Gets a string representation of the value. May be null.
        /// </summary>
        /// <returns></returns>
        //public override string ToString() => Value?.ToString();


        /// <summary>
        /// Gets a string representation of the value. May be null.
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        string ToString(string format);

        /// <summary>
        /// Builds a <see cref="ITpsObject"/> from the given binary reader and field definition information.
        /// </summary>
        /// <param name="rx">The binary reader.</param>
        /// <param name="encoding">The text encoding to use when reading string values.</param>
        /// <param name="enumerator">An enumerator for a collection of field definitions, the first being the field to parse, followed by the remainder of the definitions.
        /// The enumerator must have already been advanced to the first item with a call to <see cref="IEnumerator.MoveNext"/>.</param>
        /// <returns></returns>
        internal static ITpsObject ParseField(TpsReader rx, Encoding encoding, FieldDefinitionEnumerator enumerator)
        {
            if (rx == null)
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

            if (current.IsArray)
            {
                return TpsArrayExtensions.Parse(rx, encoding, enumerator);
            }
            else
            {
                return ParseScalarField(rx, encoding, enumerator);
            }
        }

        private static ITpsObject ParseScalarField(TpsReader rx, Encoding encoding, FieldDefinitionEnumerator enumerator) =>
            ParseScalarField(
                rx: rx,
                encoding: encoding,
                length: enumerator.Current?.Length ?? throw new ArgumentException("The current element is null.", nameof(enumerator)),
                enumerator: enumerator);

        internal static ITpsObject ParseScalarField(TpsReader rx, Encoding encoding, int length, FieldDefinitionEnumerator enumerator)
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

            switch (current.Type)
            {
                case TpsTypeCode.Byte:
                    return rx.ReadTpsByte();
                case TpsTypeCode.Short:
                    return rx.ReadTpsShort();
                case TpsTypeCode.UShort:
                    return rx.ReadTpsUnsignedShort();
                case TpsTypeCode.Date:
                    return rx.ReadTpsDate();
                case TpsTypeCode.Time:
                    return rx.ReadTpsTime();
                case TpsTypeCode.Long:
                    return rx.ReadTpsLong();
                case TpsTypeCode.ULong:
                    return rx.ReadTpsUnsignedLong();
                case TpsTypeCode.SReal:
                    return rx.ReadTpsFloat();
                case TpsTypeCode.Real:
                    return rx.ReadTpsDouble();
                case TpsTypeCode.Decimal:
                    return rx.ReadTpsDecimal(length, current.BcdDigitsAfterDecimalPoint);
                case TpsTypeCode.String:
                    return rx.ReadTpsString(encoding, length);
                case TpsTypeCode.CString:
                    return rx.ReadTpsCString(encoding);
                case TpsTypeCode.PString:
                    return rx.ReadTpsPString(encoding);
                case TpsTypeCode.Group:
                    return TpsGroup.BuildFromFieldDefinitions(rx, encoding, enumerator);
                default:
                    throw new ArgumentException($"Unsupported type {current.Type} ({length})", nameof(enumerator));
            }
        }

        ///// <inheritdoc/>
        //public sealed override bool Equals(object obj)
        //{
        //    if (obj is TpsObject o)
        //    {
        //        return Value?.Equals(o.Value) ?? false;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        ///// <inheritdoc/>
        //public override int GetHashCode()
        //{
        //    var hashCode = -1431579180;
        //    hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(Value);
        //    hashCode = hashCode * -1521134295 + TypeCode.GetHashCode();
        //    return hashCode;
        //}

        ///// <inheritdoc/>
        //public static bool operator ==(TpsObject left, TpsObject right)
        //{
        //    return EqualityComparer<TpsObject>.Default.Equals(left, right);
        //}

        ///// <inheritdoc/>
        //public static bool operator !=(TpsObject left, TpsObject right)
        //{
        //    return !(left == right);
        //}
    }

    /// <summary>
    /// Represents a typed object within the TopSpeed file.
    /// </summary>
    /// <typeparam name="T">The .NET equivalent type of the value this object encapsulates.</typeparam>
    public abstract class TpsObject<T> : TpsObject
    {
        /// <summary>
        /// Gets the .NET equivalent object value that represents this type.
        /// </summary>
        public T Value
        {
            get => _typedValue;
            protected set
            {
                _typedValue = value;
            }
        }
        private T _typedValue;

        /// <inheritdoc/>
        public override string ToString() => Value?.ToString();
    }
}
