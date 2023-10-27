namespace UserProfileService.Sync.Abstraction.Configurations;

/// <summary>
///     Defines a specific ldap query and the search base where
///     the filters should apply.
/// </summary>
public interface ILdapQueries
{
    /// <summary>
    ///     A specific ldap filter that is used to get object from the
    ///     active directory.
    /// </summary>
    string Filter { set; get; }

    /// <summary>
    ///     The search base from where the filter should apply.
    /// </summary>
    string SearchBase { set; get; }
}
