using System.Collections.Generic;
using System.Linq;
using UserProfileService.Sync.Abstraction.Models;

namespace UserProfileService.Sync.Extensions;

/// <summary>
///     Contains extension methods related to <see cref="IDictionary{TKey,TValue}" />s.
/// </summary>
public static class DictionaryExtension
{
    /// <summary>
    ///     Converts the given dictionary into the <see cref="KeyProperties" /> using the first key.
    /// </summary>
    /// <param name="dict">Dictionary to convert to.</param>
    /// <returns>Converted instance of <see cref="KeyProperties" />.</returns>
    public static KeyProperties ToKeyProperties(this IDictionary<string, string> dict)
    {
        KeyValuePair<string, string> firstId = dict.FirstOrDefault();

        return new KeyProperties(firstId.Value, firstId.Key);
    }
}
