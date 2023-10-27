using System;
using System.Collections.Generic;
using System.Linq;

namespace UserProfileService.Common.V2.Extensions;

/// <summary>
///     Contains methods that extend <see cref="Enum" />s.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    ///     Gets the single values of a flag enum as list of the same type.
    /// </summary>
    /// <typeparam name="TEnum">The type of the flag <see cref="Enum" />.</typeparam>
    /// <param name="input">The instance of the <see cref="Enum" />.</param>
    /// <returns>A list of single values of <paramref name="input" />.</returns>
    public static List<TEnum> GetSingleFlagValues<TEnum>(this TEnum input)
        where TEnum : Enum
    {
        return ((TEnum[])Enum.GetValues(typeof(TEnum)))
            .Where(val => input.HasFlag(val))
            .ToList();
    }
}
