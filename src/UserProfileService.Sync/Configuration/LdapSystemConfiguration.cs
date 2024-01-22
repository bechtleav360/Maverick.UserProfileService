using System.Collections.Generic;
using UserProfileService.Sync.Abstraction.Annotations;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstraction.Configurations.Implementations;
using UserProfileService.Sync.Abstraction.Contracts;

namespace UserProfileService.Sync.Configuration;

/// <summary>
///     The hole active directory configuration is defined in this interface.
/// </summary>
[SynchConfigurationProvider(SyncConstants.System.Ldap)]
public class LdapSystemConfiguration
{
    /// <summary>
    ///     The active directory configuration.
    /// </summary>
    public ActiveDirectory[] LdapConfiguration { get; set; }
    
    /// <summary>
    ///     The mapping between the ldap attributes and the model
    ///     properties. Key: ModelMaverickObject --> Value: LdapAttribute
    /// </summary>
    public Dictionary<string, string> EntitiesMapping { get; set; }
}
