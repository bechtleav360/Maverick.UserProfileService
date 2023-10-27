using System.Collections.Generic;

namespace UserProfileService.Sync.Abstraction.Configurations.Implementations;

/// <inheritdoc />
public class ActiveDirectory : ILdap
{
    /// <inheritdoc />
    public ActiveDirectoryConnection Connection { get; set; }

    /// <summary>
    ///     Maps the ldap attributes to the properties
    ///     for our object.
    /// </summary>
    public Dictionary<string, string> EntitiesMapping { get; set; }

    /// <inheritdoc />
    public LdapQueries[] LdapQueries { get; set; }
}
