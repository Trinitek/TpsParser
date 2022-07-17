using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TpsParser.Tps.Type;

namespace TpsParser
{
    /// <summary>
    /// Contains deserialization options for mapping to members of a particular type.
    /// </summary>
    public abstract class TypeMapOptions
    {
        /// <summary>
        /// Gets an expression that interprets a <see cref="ITpsObject"/> as some value based on the set options.
        /// </summary>
        /// <param name="fallbackValue"></param>
        /// <returns></returns>
        protected internal abstract Expression<Func<ITpsObject, object>> CreateValueInterpreter(object fallbackValue);

        internal static Dictionary<Type, Expression<Func<ITpsObject, object>>> DefaultInterpreters { get; } =
            new Dictionary<Type, Expression<Func<ITpsObject, object>>>
            {
                //[typeof(DateTime)] = x => x.ToDateTime().Value ?? default,
                //[typeof(DateTime?)] = x => x.ToDateTime().Value,
                //[typeof(TimeSpan)] = x => x.ToTimeSpan().Value,
                //[typeof(TimeSpan?)] = x => x.ToTimeSpan().AsNullable(),
                //[typeof(bool)] = x => x.ToBoolean().Value,
                //[typeof(bool?)] = x => x.ToBoolean().AsNullable(),
                //[typeof(string)] = x => x.ToString(),
                //[typeof(decimal)] = x => x.ToDecimal().Value,
                //[typeof(decimal?)] = x => x.ToDecimal().AsNullable(),
                //[typeof(int)] = x => x.ToInt32().Value,
                //[typeof(int?)] = x => x.ToInt32().AsNullable(),
                //[typeof(short)] = x => x.ToInt16().Value,
                //[typeof(short?)] = x => x.ToInt16().AsNullable(),
                //[typeof(long)] = x => x.ToInt64().Value,
                //[typeof(long?)] = x => x.ToInt64().AsNullable(),
                //[typeof(sbyte)] = x => x.ToSByte().Value,
                //[typeof(sbyte?)] = x => x.ToSByte().AsNullable(),
                //[typeof(uint)] = x => x.ToUInt32().Value,
                //[typeof(uint?)] = x => x.ToUInt32().AsNullable(),
                //[typeof(ushort)] = x => x.ToUInt16().Value,
                //[typeof(ushort?)] = x => x.ToUInt16().AsNullable(),
                //[typeof(ulong)] = x => x.ToUInt64().Value,
                //[typeof(ulong?)] = x => x.ToUInt64().AsNullable(),
                //[typeof(byte)] = x => x.ToByte().Value,
                //[typeof(byte?)] = x => x.ToByte().AsNullable(),
            };

        /// <summary>
        /// Represents an unset value.
        /// </summary>
        public static object UnsetValue { get; } = new object();
    }
}
