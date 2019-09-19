using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TpsParser
{
    internal sealed class DeserializerContext
    {
        public DeserializerContext()
        { }

        public IEnumerable<ModelMember> GetModelMembers<T>(T targetObject) where T : class
        {
            if (targetObject is null)
            {
                throw new ArgumentNullException(nameof(targetObject));
            }

            var members = typeof(T).GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            var modelMembers = members
                .Select(m => ModelMember.BuildModelMember(m))
                .Where(m => m != null);

            return modelMembers;
        }
    }

    internal sealed class ModelMember
    {
        public bool IsRecordNumber { get; }

        public TpsFieldAttribute FieldAttribute { get; }

        public MemberInfo MemberInfo { get; }

        public static ModelMember BuildModelMember(MemberInfo memberInfo)
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
                return new ModelMember(memberInfo, null);
            }
            else if (tpsFieldAttribute != null)
            {
                return new ModelMember(memberInfo, tpsFieldAttribute);
            }
            else
            {
                return null;
            }
        }

        private ModelMember(MemberInfo memberInfo, TpsFieldAttribute fieldAttribute)
        {
            MemberInfo = memberInfo;
            FieldAttribute = fieldAttribute;
            IsRecordNumber = FieldAttribute is null;
        }

        public void SetMember<T>(T targetObject, object value)
        {
            if (MemberInfo is PropertyInfo prop)
            {
                if (!prop.CanWrite)
                {
                    throw new TpsParserException($"The property '{MemberInfo.Name}' must have a setter.");
                }

                prop.SetValue(targetObject, value);
            }
            else if (MemberInfo is FieldInfo field)
            {
                field.SetValue(targetObject, value);
            }
        }
    }
}
