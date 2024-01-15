using System.Collections.Generic;
using Maverick.UserProfileService.Models.Abstraction;
using UserProfileService.Sync.Abstraction.Configurations.Implementations;

namespace UserProfileService.Sync.Extensions.Ldap;

/// <summary>
///     Contains extension methods related to <see cref="ActiveDirectoryConnection" />s.
/// </summary>
internal static class ActiveDirectoryMappingExtensions {
    /// <summary>
    ///     Decides which attribute to take to extract the SID out of the profile.
    /// </summary>
    /// <param name="settings">The setting for the active directory.</param>
    /// <returns></returns>
    public static string GetProfileIdLdapKey(this IDictionary<string,string> settings)
    {
        if (settings == null)
        {
            return "objectSid";
        }

        return settings.TryGetValue(
            nameof(IProfile.Id),
            out string alternativeSidKey)
            ? alternativeSidKey
            : "objectSid";
    }
}
