using System.Collections.Generic;

namespace UserProfileService.Sync.Abstraction.Configurations.Implementations;

/// <summary>
///     The general system configuration holds all configuration that are necessary for the
///     synchronization process from the source system. It also defines which item are allowed
///     to changed in the source system.
/// </summary>
public class GeneralSystemConfiguration : ILdapConfiguration
{
    /// <summary>
    ///     The mapping between the ldap attributes and the model
    ///     properties. Key: ModelMaverickObject --> Value: LdapAttribute
    /// </summary>
    public Dictionary<string, string> EntitiesMapping { get; set; }

    /// <inheritdoc />
    public ActiveDirectory[] LdapConfiguration { get; set; }
}
