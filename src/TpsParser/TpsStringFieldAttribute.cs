using System;
using System.Reflection;
using TpsParser.Tps.Type;

namespace TpsParser
{
    /// <summary>
    /// <para>
    /// Marks the property or field as a TopSpeed field, MEMO, or BLOB. This attribute is intended for use on <see cref="string"/>
    /// members to provide trimming and formatting options. Strings are trimmed by default unless explicitly disabled.
    /// </para>
    /// <para>
    /// If present on a field, the field may be private.
    /// </para>
    /// <para>
    /// If present on a property, the property must have a setter. The setter may be private.
    /// </para>
    /// </summary>
    public sealed class TpsStringFieldAttribute : TpsFieldAttribute
    {
        /// <summary>
        /// True if the end of the string should be trimmed. This is useful when converting from <see cref="TpsString"/>
        /// values as those strings are padded with whitespace up to their total lengths. This is true by default.
        /// </summary>
        public bool TrimEnd { get; set; } = true;

        /// <summary>
        /// Gets or sets the string format to use when calling ToString() on a non-string type.
        /// </summary>
        public string StringFormat { get; set; }

        /// <summary>
        /// Marks the property or field as a TopSpeed field, MEMO, or BLOB.
        /// </summary>
        /// <param name="fieldName">The case insensitive name of the column.</param>
        public TpsStringFieldAttribute(string fieldName)
            : base(fieldName)
        { }

        internal override object InterpretValue(MemberInfo member, TpsObject sourceObject)
        {
            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            Type memberType = GetMemberType(member);

            if (memberType != typeof(string))
            {
                throw new TpsParserException($"{nameof(TpsStringFieldAttribute)} is only valid on members of type {typeof(string)} ({member}).");
            }

            object tpsValue = sourceObject?.Value;

            if (tpsValue is null)
            {
                return FallbackValue;
            }
            else if (TrimEnd && tpsValue is string tpsStringValue)
            {
                return tpsStringValue.TrimEnd();
            }
            else
            {
                try
                {
                    if (tpsValue.GetType().GetMethod("ToString", new Type[] { typeof(string) }) is var miStringParam && miStringParam != null)
                    {
                        return (string)miStringParam.Invoke(tpsValue, new string[] { StringFormat });
                    }
                    else if (tpsValue.GetType().GetMethod("ToString", Type.EmptyTypes) is var miNoParams && miNoParams != null)
                    {
                        return (string)miNoParams.Invoke(tpsValue, null);
                    }
                    else
                    {
                        throw new TpsParserException("No suitable ToString method was found on the object. This is probably a bug in the library.");
                    }
                }
                catch (Exception ex)
                {
                    throw new TpsParserException($"Unable to apply string conversion on the given member. See the inner exception for details. ({member})", ex);
                }
            }
        }
    }
}
