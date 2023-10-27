namespace UserProfileService.Sync.Abstraction.Models;

/// <summary>
///     The base profile interface
/// </summary>
public interface ISyncProfile : ISyncModel
{
    /// <summary>
    ///     The name for displaying
    /// </summary>
    public string DisplayName { set; get; }

    /// <summary>
    ///     A profile kind is used to identify a profile. Either it is group, a user or an organization.
    /// </summary>
    public ProfileKind Kind { set; get; }

    /// <summary>
    ///     The desired name of the profile.
    /// </summary>
    public string Name { get; set; }
}
