namespace UserProfileService.Sync.Abstraction.Configurations.Implementations;

/// <summary>
///     Defines a specific ldap query and the search base where
///     the filters should apply.
/// </summary>
public class LdapQueries : ILdapQueries
{
    /// <inheritdoc />
    public string Filter { get; set; }

    /// <inheritdoc />
    public string SearchBase { get; set; }
}
