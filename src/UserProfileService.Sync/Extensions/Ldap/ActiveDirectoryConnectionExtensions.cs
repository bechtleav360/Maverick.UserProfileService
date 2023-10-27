using Maverick.UserProfileService.Models.Abstraction;
using UserProfileService.Sync.Abstraction.Configurations.Implementations;

namespace UserProfileService.Sync.Extensions.Ldap;

/// <summary>
///     Contains extension methods related to <see cref="ActiveDirectoryConnection" />s.
/// </summary>
internal static class ActiveDirectoryConnectionExtensions
{
    /// <summary>
    ///     Decides which attribute to take to extract the SID out of the profile.
    /// </summary>
    /// <param name="settings">The setting for the active directory.</param>
    /// <returns></returns>
    public static string GetProfileIdLdapKey(this ActiveDirectoryConnection settings)
    {
        if (settings?.ProfileMapping == null)
        {
            return "objectSid";
        }

        return settings.ProfileMapping.TryGetValue(
            nameof(IProfile.Id),
            out string alternativeSidKey)
            ? alternativeSidKey
            : "objectSid";
    }
}
