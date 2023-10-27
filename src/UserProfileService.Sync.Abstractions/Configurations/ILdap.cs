using UserProfileService.Sync.Abstraction.Configurations.Implementations;

namespace UserProfileService.Sync.Abstraction.Configurations;

/// <summary>
///     The active directory configuration to login to a specific
///     active directory and get the objects from a search base
///     with a specific query.
/// </summary>
public interface ILdap
{
    /// <summary>
    ///     The active directory connection object to connect with.
    /// </summary>
    ActiveDirectoryConnection Connection { set; get; }

    /// <summary>
    ///     The specific ldap queries to get the specified object.
    /// </summary>
    LdapQueries[] LdapQueries { set; get; }
}
