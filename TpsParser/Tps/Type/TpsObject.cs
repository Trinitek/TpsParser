using System.Collections.Generic;

namespace TpsParser.Tps.Type
{
    public abstract class TpsObject
    {
        public object Value { get; protected set; }

        public string TypeName => TypeCode.ToString();

        public abstract TpsTypeCode TypeCode { get; }

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
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TypeName);
            hashCode = hashCode * -1521134295 + TypeCode.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(TpsObject left, TpsObject right)
        {
            return EqualityComparer<TpsObject>.Default.Equals(left, right);
        }

        public static bool operator !=(TpsObject left, TpsObject right)
        {
            return !(left == right);
        }
    }

    public abstract class TpsObject<T> : TpsObject
    {
        public new T Value
        {
            get => (T)base.Value;
            set => base.Value = value;
        }
    }
}
