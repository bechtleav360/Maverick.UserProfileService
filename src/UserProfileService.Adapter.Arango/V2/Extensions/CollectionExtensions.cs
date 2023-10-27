using System;

namespace UserProfileService.Adapter.Arango.V2.Extensions;

internal static class CollectionExtensions
{
    internal static string GetPrefixedCollectionName(this string collectionName, string prefix)
    {
        if (collectionName == null)
        {
            throw new ArgumentNullException(nameof(collectionName));
        }

        if (prefix == null)
        {
            throw new ArgumentNullException(nameof(prefix));
        }

        return $"{prefix.Trim()}{collectionName}";
    }
}
