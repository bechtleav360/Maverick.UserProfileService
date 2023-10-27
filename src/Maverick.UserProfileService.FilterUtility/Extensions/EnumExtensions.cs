using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Maverick.UserProfileService.FilterUtility.Models;
using Maverick.UserProfileService.Models.Annotations;

namespace Maverick.UserProfileService.FilterUtility.Extensions
{
    internal static class EnumExtensions
    {
        /// <summary>
        ///     Returns the serialization values for an enum
        /// </summary>
        /// <param name="enumType">
        ///     <see cref="Enum" />
        /// </param>
        /// <returns>
        ///     <see cref="EnumFilterDefinition" />
        /// </returns>
        internal static EnumFilterDefinition GetEnumFilterDefinition(this Type enumType)
        {
            if (!enumType.IsValueType || !typeof(IConvertible).IsAssignableFrom(enumType))
            {
                throw new ArgumentException("Only enum types are allowed.", nameof(enumType));
            }

            return new EnumFilterDefinition(
                (enumType.GetCustomAttribute(typeof(FilterEncapsulateAttribute)) as FilterEncapsulateAttribute)
                ?.EncapsulateValues
                ?? true,
                enumType.GetMembers(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => m.DeclaringType == enumType)
                    .Select(
                        x =>
                            (x.GetCustomAttribute(typeof(FilterSerializeAttribute)) as FilterSerializeAttribute)
                            ?.SerializationValue
                            ?? x.Name)
                    .ToList());
        }

        /// <summary>
        ///     Gets the corresponding enum value for a string value
        /// </summary>
        /// <typeparam name="TEnum">The enum type</typeparam>
        /// <param name="match">The value to parse</param>
        /// <returns></returns>
        internal static TEnum ParseFilterSerializeAttribute<TEnum>(string match) where TEnum : Enum
        {
            if (string.IsNullOrWhiteSpace(match))
            {
                throw new SerializationException(
                    $"Unable to parse enum {typeof(TEnum).Name}, because input string is null or whitespace.")
                {
                    Data =
                    {
                        { "targetType", typeof(TEnum) }
                    }
                };
            }

            object result = typeof(TEnum)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(
                    m => m.DeclaringType == typeof(TEnum)
                        && ((m.GetCustomAttribute(typeof(FilterSerializeAttribute)) as
                                FilterSerializeAttribute)
                            ?.SerializationValue
                            ?? m.Name).Equals(match.Trim(), StringComparison.OrdinalIgnoreCase))
                ?
                .GetValue(null);

            if (result == null)
            {
                throw new SerializationException(
                    $"Unable to parse matched result to enum {typeof(TEnum).Name}. Input string: \"{match}\". Possible values: \"{GetValueString<TEnum>()}\"")
                {
                    Data =
                    {
                        { "value", match },
                        { "targetType", typeof(TEnum) }
                    }
                };
            }

            return (TEnum)result;
        }

        internal static string GetValueString<TEnum>()
            where TEnum : Enum
        {
            return string.Join("\",\"", ((TEnum[])Enum.GetValues(typeof(TEnum))).Select(v => v.ToString("G")));
        }

        public static TAttribute GetAttribute<TAttribute>(this Enum value)
            where TAttribute : Attribute
        {
            Type enumType = value.GetType();
            string name = Enum.GetName(enumType, value);

            return enumType.GetField(name).GetCustomAttributes(false).OfType<TAttribute>().SingleOrDefault();
        }
    }
}
