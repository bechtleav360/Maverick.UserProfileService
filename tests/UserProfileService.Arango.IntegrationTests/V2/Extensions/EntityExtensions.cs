using System.Collections.Generic;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.IntegrationTests.V2.Helpers;

namespace UserProfileService.Arango.IntegrationTests.V2.Extensions
{
    public static class EntityExtensions
    {
        internal static CustomPropertyEntityModel GetCustomPropertyOfProfile(
            this IProfileEntityModel profile,
            string key)
        {
            return !string.IsNullOrWhiteSpace(profile?.Id)
                ? SampleDataTestHelper.GetCustomPropertyOfProfile(profile.Id, key)
                : default;
        }

        internal static List<CustomPropertyEntityModel> GetCustomPropertiesOfProfile(this IProfileEntityModel profile)
        {
            return !string.IsNullOrWhiteSpace(profile?.Id)
                ? SampleDataTestHelper.GetCustomPropertiesOfProfile(profile.Id)
                : default;
        }

        internal static bool HasCustomProperty(
            this IProfileEntityModel profile,
            string key)
        {
            return GetCustomPropertyOfProfile(profile, key) != null;
        }

        internal static int CountCustomProperties(this IProfileEntityModel profile)
        {
            return GetCustomPropertiesOfProfile(profile)?.Count ?? 0;
        }
    }
}
