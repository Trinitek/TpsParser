using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TpsParser.Binary;
using TpsParser.Tps.Record;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a typed object within the TopSpeed file.
    /// </summary>
    public abstract class TpsObject : IConvertible<bool>
    {
        /// <summary>
        /// Gets the .NET equivalent value of the TopSpeed object.
        /// </summary>
        public object Value { get; protected set; }

        /// <summary>
        /// Gets the type code of the object.
        /// </summary>
        public abstract TpsTypeCode TypeCode { get; }

        /// <summary>
        /// Gets a boolean representation of the value as governed by Clarion logic evaluation rules for the type.
        /// </summary>
        /// <returns></returns>
        internal abstract bool AsBoolean();

        /// <summary>
        /// Gets the string representation of the value that this object encapsulates.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Value?.ToString();

        /// <summary>
        /// Builds a <see cref="TpsObject"/> from the given binary reader and field definition information.
        /// </summary>
        /// <param name="rx">The binary reader.</param>
        /// <param name="encoding">The text encoding to use when reading string values.</param>
        /// <param name="length">The length of the field value in bytes.</param>
        /// <param name="remainingFieldDefinitions">A collection of field definitions, the first being the field to parse, followed by the remainder of the definitions.</param>
        /// 
        /// <returns></returns>
        public static TpsObject ParseField(RandomAccess rx, Encoding encoding, int length, IEnumerable<IFieldDefinitionRecord> remainingFieldDefinitions)
        {
            if (remainingFieldDefinitions is null)
            {
                throw new ArgumentNullException(nameof(remainingFieldDefinitions));
            }

            var currentFieldDefinition = remainingFieldDefinitions.First();

            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            switch (currentFieldDefinition.Type)
            {
                case TpsTypeCode.Byte:
                    AssertExpectedLength(1, length);
                    return new TpsByte(rx);
                case TpsTypeCode.Short:
                    AssertExpectedLength(2, length);
                    return new TpsShort(rx);
                case TpsTypeCode.UShort:
                    AssertExpectedLength(2, length);
                    return new TpsUnsignedShort(rx);
                case TpsTypeCode.Date:
                    return new TpsDate(rx);
                case TpsTypeCode.Time:
                    return new TpsTime(rx);
                case TpsTypeCode.Long:
                    AssertExpectedLength(4, length);
                    return new TpsLong(rx);
                case TpsTypeCode.ULong:
                    AssertExpectedLength(4, length);
                    return new TpsUnsignedLong(rx);
                case TpsTypeCode.SReal:
                    AssertExpectedLength(4, length);
                    return new TpsFloat(rx);
                case TpsTypeCode.Real:
                    AssertExpectedLength(8, length);
                    return new TpsDouble(rx);
                case TpsTypeCode.Decimal:
                    return new TpsDecimal(rx, length, currentFieldDefinition.BcdDigitsAfterDecimalPoint);
                case TpsTypeCode.String:
                    return new TpsString(rx, length, encoding);
                case TpsTypeCode.CString:
                    return new TpsCString(rx, encoding);
                case TpsTypeCode.PString:
                    return new TpsPString(rx, encoding);
                case TpsTypeCode.Group:
                    return TpsGroup.BuildFromFieldDefinitions(rx, encoding, remainingFieldDefinitions);
                default:
                    throw new ArgumentException($"Unsupported type {currentFieldDefinition.Type} ({length})", nameof(remainingFieldDefinitions));
            }
        }

        private static void AssertExpectedLength(int expected, int actual)
        {
            if (expected != actual)
            {
                throw new ArgumentException($"Expected length of {expected} but was {actual}.");
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public override bool Equals(object obj)
        {
            if (obj is TpsObject o)
            {
                return Value?.Equals(o.Value) ?? false;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            var hashCode = -1431579180;
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(Value);
            hashCode = hashCode * -1521134295 + TypeCode.GetHashCode();
            return hashCode;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1033:Interface methods should be callable by child types", Justification = "This must remain explicitly implemented because derivative classes may add their own IConvertible implementations for different types.")]
        bool IConvertible<bool>.AsType() => AsBoolean();

        public static bool operator ==(TpsObject left, TpsObject right)
        {
            return EqualityComparer<TpsObject>.Default.Equals(left, right);
        }

        public static bool operator !=(TpsObject left, TpsObject right)
        {
            return !(left == right);
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    /// <summary>
    /// Represents a typed object within the TopSpeed file.
    /// </summary>
    /// <typeparam name="T">The .NET equivalent type of the value this object encapsulates.</typeparam>
    public abstract class TpsObject<T> : TpsObject, IConvertible<T>
    {
        /// <inheritdoc/>
        public new T Value
        {
            get => _typedValue;
            protected set
            {
                _typedValue = value;
                base.Value = value;
            }
        }
        private T _typedValue;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1033:Interface methods should be callable by child types", Justification = "Child types should use the Value property instead.")]
        T IConvertible<T>.AsType() => Value;
    }
}
