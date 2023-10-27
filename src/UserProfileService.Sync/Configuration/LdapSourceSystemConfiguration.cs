using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstraction.Configurations.Implementations;

namespace UserProfileService.Sync.Configuration;

/// <summary>
///     The Ldap configuration.
/// </summary>
public class LdapSourceSystemConfiguration : ILdapConfiguration
{
    /// <inheritdoc />
    public ActiveDirectory[] LdapConfiguration { get; set; }
}
