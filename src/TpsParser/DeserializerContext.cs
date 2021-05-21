using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TpsParser.Tps.Type;

namespace TpsParser
{
    internal sealed class DeserializerContext
    {
        private Dictionary<Type, ModelMember[]> TypeCache { get; }
        private StringOptions DefaultStringOptions { get; }
        private BooleanOptions DefaultBooleanOptions { get; }

        public DeserializerContext(StringOptions defaultStringOptions, BooleanOptions defaultBooleanOptions)
        {
            DefaultStringOptions = defaultStringOptions ?? throw new ArgumentNullException(nameof(defaultStringOptions));
            DefaultBooleanOptions = defaultBooleanOptions ?? throw new ArgumentNullException(nameof(defaultBooleanOptions));
            
            TypeCache = new Dictionary<Type, ModelMember[]>();
        }

        public IEnumerable<ModelMember<T>> GetModelMembers<T>(T targetObject) where T : class
        {
            if (TypeCache.TryGetValue(typeof(T), out var members))
            {
                return (IEnumerable<ModelMember<T>>)members;
            }
            else
            {
                return Add(targetObject);
            }
        }

        private IEnumerable<ModelMember<T>> Add<T>(T targetObject) where T : class
        {
            if (targetObject is null)
            {
                throw new ArgumentNullException(nameof(targetObject));
            }

            var members = typeof(T).GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            var modelMembers = members
                .Select(m => ModelMember<T>.BuildModelMember(m, DefaultStringOptions, DefaultBooleanOptions))
                .Where(m => m != null)
                .ToArray();

            TypeCache.Add(typeof(T), modelMembers);

            return modelMembers;
        }
    }

    internal abstract class ModelMember
    { }

    internal sealed class ModelMember<TModel> : ModelMember
    {
        public bool IsRecordNumber { get; }

        public TpsFieldAttribute FieldAttribute { get; }

        public Type MemberType { get; }

        public TypeMapOptions MapOptions { get; }

        private Action<TModel, TpsObject> Setter { get; set; }

        private static Type GetMemberType(MemberInfo info)
        {
            if (info is PropertyInfo pi)
            {
                return pi.PropertyType;
            }
            else if (info is FieldInfo fi)
            {
                return fi.FieldType;
            }
            else
            {
                throw new InvalidOperationException($"Expected PropertyInfo or FieldInfo, was {info?.GetType()}");
            }
        }

        private static bool IsSingleOrArray<T>(Type subject)
            => subject == typeof(T) || subject.IsAssignableFrom(typeof(T[]));

        private static TypeMapOptions GetOptions<T>(Type subject, TypeMapOptions specific, TypeMapOptions fallback)
            => IsSingleOrArray<T>(subject) ? specific ?? fallback : null;

        public static ModelMember<TModel> BuildModelMember(
            MemberInfo memberInfo,
            StringOptions defaultStringOptions,
            BooleanOptions defaultBooleanOptions)
        {
            if (memberInfo is null)
            {
                throw new ArgumentNullException(nameof(memberInfo));
            }

            if (defaultStringOptions is null)
            {
                throw new ArgumentNullException(nameof(defaultStringOptions));
            }

            if (defaultBooleanOptions is null)
            {
                throw new ArgumentNullException(nameof(defaultBooleanOptions));
            }

            var tpsFieldAttribute = memberInfo.GetCustomAttribute<TpsFieldAttribute>();
            var tpsRecordNumberAttribute = memberInfo.GetCustomAttribute<TpsRecordNumberAttribute>();

            if (tpsFieldAttribute != null && tpsRecordNumberAttribute != null)
            {
                throw new TpsParserException($"Members cannot be marked with both {nameof(TpsFieldAttribute)} and {nameof(TpsRecordNumberAttribute)}. Property name '{memberInfo.Name}'.");
            }

            if (tpsRecordNumberAttribute is null && tpsFieldAttribute is null)
            {
                return null;
            }

            var stringOpt = memberInfo.GetCustomAttribute<StringOptionsAttribute>()?.GetOptions();
            var booleanOpt = memberInfo.GetCustomAttribute<BooleanOptionsAttribute>()?.GetOptions();

            var mType = GetMemberType(memberInfo);

            //if (stringOpt != null && mType != typeof(string) && !mType.IsAssignableFrom(typeof(string[])))
            if (stringOpt != null && !IsSingleOrArray<string>(mType))
            {
                throw new TpsParserException($"{nameof(StringOptions)} is only valid on members and collections of type {typeof(string)}.");
            }

            //if (booleanOpt != null && mType != typeof(bool) && !mType.IsAssignableFrom(typeof(bool[])))
            if (booleanOpt != null && !IsSingleOrArray<bool>(mType))
            {
                throw new TpsParserException($"{nameof(BooleanOptions)} is only valid on members and collections of type {typeof(bool)}.");
            }

            TypeMapOptions assignableMapOptions =
                GetOptions<string>(mType, stringOpt, defaultStringOptions)
                ?? GetOptions<bool>(mType, booleanOpt, defaultBooleanOptions);

            return new ModelMember<TModel>(
                memberInfo,
                tpsFieldAttribute,
                assignableMapOptions);
        }

        private ModelMember(
            MemberInfo memberInfo,
            TpsFieldAttribute fieldAttribute,
            TypeMapOptions mapOptions)
        {
            if (memberInfo is null)
            {
                throw new ArgumentNullException(nameof(memberInfo));
            }

            FieldAttribute = fieldAttribute;
            IsRecordNumber = FieldAttribute is null;

            MapOptions = mapOptions;

            var targetParamExpr = Expression.Parameter(typeof(TModel));
            var tpsObjParamExpr = Expression.Parameter(typeof(TpsObject));

            MemberExpression memberExpression;

            if (memberInfo is PropertyInfo property)
            {
                if (!property.CanWrite)
                {
                    throw new TpsParserException($"The property '{memberInfo.Name}' must have a setter.");
                }

                MemberType = property.PropertyType;

                memberExpression = Expression.Property(targetParamExpr, property);
            }
            else if (memberInfo is FieldInfo field)
            {
                MemberType = field.FieldType;

                memberExpression = Expression.Field(targetParamExpr, field);
            }
            else
            {
                throw new TpsParserException($"Tried to create a {nameof(ModelMember)} for a member that is not a property or field. (Member {memberInfo} declared in {memberInfo.DeclaringType})");
            }

            Expression<Func<TpsObject, object>> getTpsObjValueExpr;

            if (MapOptions != null)
            {
                getTpsObjValueExpr = MapOptions.CreateValueInterpreter(FieldAttribute.FallbackValue);
            }
            else if (!TypeMapOptions.DefaultInterpreters.TryGetValue(MemberType, out getTpsObjValueExpr))
            {
                if (typeof(TpsObject).IsAssignableFrom(MemberType))
                {
                    // Cast may fail if the member is, for example, TpsLong but the field is TpsByte.
                    // TPS field types are not known at this point.

                    getTpsObjValueExpr = x => ReferenceEquals(x, null) ? FieldAttribute.FallbackValue : x;
                }
                else if (MapOptions != null)
                {
                    getTpsObjValueExpr = MapOptions.CreateValueInterpreter(FieldAttribute.FallbackValue);
                }
                else
                {
                    getTpsObjValueExpr = x => ReferenceEquals(x, null) ? FieldAttribute.FallbackValue : x.Value;
                }
            }

            // Builds the expression...
            // targetObject.SomeProperty = (MemberType)GetTpsObjValue(tpsObj);

            var assignmentExpr = Expression.Assign(
                    memberExpression,
                    Expression.Convert(
                        Expression.Invoke(
                            getTpsObjValueExpr,
                            tpsObjParamExpr),
                        MemberType));

            var lambda = Expression.Lambda<Action<TModel, TpsObject>>(assignmentExpr, targetParamExpr, tpsObjParamExpr).Compile();

            Setter = lambda;
        }

        public void SetMember(TModel targetObject, TpsObject sourceValue)
        {
            if (sourceValue is ITpsArray)
            {
                // TODO
                throw new NotImplementedException();
            }
            else if (sourceValue is TpsGroup)
            {
                // TODO
                throw new NotImplementedException();
            }
            else
            {
                Setter.Invoke(targetObject, sourceValue);
            }
        }

        //public void SetMember(TModel targetObject, TpsObject sourceValue) => Setter.Invoke(targetObject, sourceValue);
    }
}
