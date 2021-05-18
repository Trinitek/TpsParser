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

        public DeserializerContext()
        {
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
                .Select(m => ModelMember<T>.BuildModelMember(m))
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

        public StringOptions StringOptions { get; }
        public BooleanOptions BooleanOptions { get; }

        private Action<TModel, TpsObject> Setter { get; set; }

        public static ModelMember<TModel> BuildModelMember(MemberInfo memberInfo)
        {
            if (memberInfo is null)
            {
                throw new ArgumentNullException(nameof(memberInfo));
            }

            var tpsFieldAttribute = memberInfo.GetCustomAttribute<TpsFieldAttribute>();
            var tpsRecordNumberAttribute = memberInfo.GetCustomAttribute<TpsRecordNumberAttribute>();

            var stringOpt = memberInfo.GetCustomAttribute<StringOptionsAttribute>()?.GetOptions();
            var booleanOpt = memberInfo.GetCustomAttribute<BooleanOptionsAttribute>()?.GetOptions();

            if (tpsFieldAttribute != null && tpsRecordNumberAttribute != null)
            {
                throw new TpsParserException($"Members cannot be marked with both {nameof(TpsFieldAttribute)} and {nameof(TpsRecordNumberAttribute)}. Property name '{memberInfo.Name}'.");
            }

            if (tpsRecordNumberAttribute is null && tpsFieldAttribute is null)
            {
                return null;
            }

            if (stringOpt != null && booleanOpt != null)
            {
                throw new TpsParserException($"Too many option attributes are specified on property '{memberInfo.Name}'.");
            }

            return new ModelMember<TModel>(
                memberInfo,
                tpsFieldAttribute,
                stringOpt,
                booleanOpt);
        }

        private ModelMember(
            MemberInfo memberInfo,
            TpsFieldAttribute fieldAttribute,
            StringOptions stringOptions,
            BooleanOptions booleanOptions)
        {
            if (memberInfo is null)
            {
                throw new ArgumentNullException(nameof(memberInfo));
            }

            FieldAttribute = fieldAttribute;
            IsRecordNumber = FieldAttribute is null;

            StringOptions = stringOptions;
            BooleanOptions = booleanOptions;

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
                throw new TpsParserException($"{nameof(TpsFieldAttribute)} is only supported on properties and fields. (Member {memberInfo} declared in {memberInfo.DeclaringType})");
            }

            if (stringOptions != null)
            {
                if (MemberType != typeof(string) || !MemberType.IsAssignableFrom(typeof(string[])))
                {
                    throw new TpsParserException($"{nameof(StringOptions)} is only valid on members and collections of type {typeof(string)}.");
                }
            }

            if (booleanOptions != null)
            {
                if (MemberType != typeof(bool) || !MemberType.IsAssignableFrom(typeof(bool[])))
                {
                    throw new TpsParserException($"{nameof(BooleanOptions)} is only valid on members and collections of type {typeof(bool)}.");
                }
            }

            if (!TpsFieldAttribute.ValueInterpreters.TryGetValue(MemberType, out var getTpsObjValueExpr))
            {
                if (typeof(TpsObject).IsAssignableFrom(MemberType))
                {
                    // Cast may fail if the member is, for example, TpsLong but the field is TpsByte.
                    // Field types are not known at this point.

                    getTpsObjValueExpr = x => ReferenceEquals(x, null) ? FieldAttribute.FallbackValue : x;
                }
                else
                {
                    //if (MemberType.IsValueType)
                    //{
                    //    // Equivalent to...
                    //    // x => x == null ? default(MemberType) : x.Value
                    //
                    //    getTpsObjValueExpr =
                    //        Expression.Lambda<Func<TpsObject, object>>(
                    //            Expression.IfThenElse(
                    //                test: Expression.ReferenceEqual(tpsObjParamExpr, Expression.Constant(null)),
                    //                ifTrue: Expression.Constant(FieldAttribute.FallbackValue), //Expression.New(MemberType),
                    //                ifFalse: Expression.Property(tpsObjParamExpr, nameof(TpsObject.Value))),
                    //            tpsObjParamExpr);
                    //}
                    //else
                    //{
                    //    getTpsObjValueExpr = x => x == null ? FieldAttribute.FallbackValue : x.Value;
                    //}

                    getTpsObjValueExpr = x => ReferenceEquals(x, null) ? FieldAttribute.FallbackValue : x.Value;
                }
            }

            // Builds the expression...
            // targetObject.SomeProperty = (MemberType)value;

            var assignmentExpr = Expression.Assign(
                    memberExpression,
                    Expression.Convert(getTpsObjValueExpr, MemberType));

            var lambda = Expression.Lambda<Action<TModel, TpsObject>>(assignmentExpr, targetParamExpr, tpsObjParamExpr).Compile();

            Setter = lambda;
        }

        public void SetMember(TModel targetObject, TpsObject sourceValue) => Setter.Invoke(targetObject, sourceValue);
    }
}
