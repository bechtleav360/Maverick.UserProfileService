using UserProfileService.Sync.Abstraction.Configurations.Implementations;

namespace UserProfileService.Sync.Abstraction.Configurations;

/// <summary>
///     The hole active directory configuration is defined in this interface.
/// </summary>
public interface ILdapConfiguration
{
    /// <summary>
    ///     The active directory configuration.
    /// </summary>
    ActiveDirectory[] LdapConfiguration { set; get; }
}
