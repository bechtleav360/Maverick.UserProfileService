using System;
using System.Reflection;

namespace UserProfileService.Common.V2.Extensions;

/// <summary>
///     Contains extension methods related to <see cref="MemberInfo" />s.
/// </summary>
public static class MemberInfoExtension
{
    /// <summary>
    ///     Returns the value of a custom attribute defined by the parameter <paramref name="valueFunc" />.
    /// </summary>
    /// <typeparam name="TAttribute">Type of custom attribute.</typeparam>
    /// <typeparam name="TValue">Return value type of parameter <paramref name="valueFunc" />.</typeparam>
    /// <param name="type">Type from which the attribute is to be returned. </param>
    /// <param name="valueFunc">Function that defines which property should be returned.</param>
    /// <returns>
    ///     Value of custom property by the parameter <paramref name="valueFunc" />. If it does not exist the default
    ///     value is returned.
    /// </returns>
    public static TValue GetCustomAttributeValue<TAttribute, TValue>(
        this MemberInfo type,
        Func<TAttribute, TValue> valueFunc) where TAttribute : Attribute
    {
        var attribute = type.GetCustomAttribute(typeof(TAttribute)) as TAttribute;

        return attribute == null ? default : valueFunc.Invoke(attribute);
    }
}
