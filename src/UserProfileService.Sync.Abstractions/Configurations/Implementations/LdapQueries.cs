namespace UserProfileService.Sync.Abstraction.Configurations.Implementations;

/// <summary>
///     Defines a specific ldap query and the search base where
///     the filters should apply.
/// </summary>
public class LdapQueries
{
    /// <summary>
    ///     The active directory connection object to connect with.
    /// </summary>
    public string Filter { get; set; }

    /// <summary>
    ///     The specific ldap queries to get the specified object.
    /// </summary>
    public string SearchBase { get; set; }
}
