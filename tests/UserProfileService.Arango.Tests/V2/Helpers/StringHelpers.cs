using System;
using UserProfileService.Adapter.Arango.V2.Contracts;

namespace UserProfileService.Arango.Tests.V2.Helpers
{
    internal static class StringHelpers
    {
        internal static string GetDefaultCollectionNameInTest(this string suggestedName)
        {
            if (suggestedName == null)
            {
                throw new ArgumentNullException(nameof(suggestedName));
            }

            if (string.IsNullOrWhiteSpace(suggestedName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(suggestedName));
            }

            return $"{WellKnownDatabaseKeys.CollectionPrefixUserProfileService}{suggestedName}";
        }

        internal static string GetCollectionNameInTest(this string suggestedName, string prefix)
        {
            return $"{(prefix ?? WellKnownDatabaseKeys.CollectionPrefixUserProfileService).Trim()}{suggestedName}";
        }
    }
}
