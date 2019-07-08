namespace TpsParser.Tps.Type
{
    public abstract class TpsObject
    {
        public object Value { get; protected set; }

        public abstract string TypeName { get; }

        public abstract int TypeCode { get; }
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
