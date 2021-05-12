using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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

    internal sealed class ModelMember<T> : ModelMember
    {
        public bool IsRecordNumber { get; }

        public TpsFieldAttribute FieldAttribute { get; }

        //public MemberInfo MemberInfo { get; }
        public Type MemberType { get; }

        private Action<T, object> Setter { get; }

        public static ModelMember<T> BuildModelMember(MemberInfo memberInfo)
        {
            if (memberInfo is null)
            {
                throw new ArgumentNullException(nameof(memberInfo));
            }

            var tpsFieldAttribute = memberInfo.GetCustomAttribute<TpsFieldAttribute>();
            var tpsRecordNumberAttribute = memberInfo.GetCustomAttribute<TpsRecordNumberAttribute>();

            if (tpsFieldAttribute != null && tpsRecordNumberAttribute != null)
            {
                throw new TpsParserException($"Members cannot be marked with both {nameof(TpsFieldAttribute)} and {nameof(TpsRecordNumberAttribute)}. Property name '{memberInfo.Name}'.");
            }

            if (tpsRecordNumberAttribute != null)
            {
                return new ModelMember<T>(memberInfo, null);
            }
            else if (tpsFieldAttribute != null)
            {
                return new ModelMember<T>(memberInfo, tpsFieldAttribute);
            }
            else
            {
                return null;
            }
        }

        private ModelMember(MemberInfo memberInfo, TpsFieldAttribute fieldAttribute)
        {
            if (memberInfo is null)
            {
                throw new ArgumentNullException(nameof(memberInfo));
            }
            //MemberInfo = memberInfo ?? throw new ArgumentNullException(nameof(memberInfo));


            FieldAttribute = fieldAttribute;
            IsRecordNumber = FieldAttribute is null;

            if (memberInfo is PropertyInfo property)
            {
                if (!property.CanWrite)
                {
                    throw new TpsParserException($"The property '{memberInfo.Name}' must have a setter.");
                }

                // Builds the expression...
                // targetObject.SomeProperty = (PropertyType)value;

                var targetExpr = Expression.Parameter(typeof(T));
                var valueExpr = Expression.Parameter(typeof(object));
                var castExpr = Expression.Convert(valueExpr, property.PropertyType);
                var propExpr = Expression.Property(targetExpr, property);
                var assignmentExpr = Expression.Assign(propExpr, castExpr);
                var lambda = Expression.Lambda<Action<T, object>>(assignmentExpr, targetExpr, valueExpr).Compile();

                Setter = lambda;
            }
            else if (memberInfo is FieldInfo field)
            {
                // Builds the expression...
                // targetObject.SomeField = (FieldType)value;

                var targetExpr = Expression.Parameter(typeof(T));
                var valueExpr = Expression.Parameter(typeof(object));
                var castExpr = Expression.Convert(valueExpr, field.FieldType);
                var fieldExpr = Expression.Field(targetExpr, field);
                var assignmentExpr = Expression.Assign(fieldExpr, castExpr);
                var lambda = Expression.Lambda<Action<T, object>>(assignmentExpr, targetExpr, valueExpr).Compile();

                Setter = lambda;
            }
            else
            {
                throw new TpsParserException($"{nameof(TpsFieldAttribute)} is only supported on properties and fields. (Member {memberInfo} declared in {memberInfo.DeclaringType})");
            }
        }

        public void SetMember(T targetObject, object value) => Setter.Invoke(targetObject, value);
    }
}
