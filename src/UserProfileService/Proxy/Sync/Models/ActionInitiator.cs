namespace UserProfileService.Proxy.Sync.Models;

/// <summary>
///     Contains the basic information of a user that triggered an action.
/// </summary>
public class ActionInitiator
{
    /// <summary>
    ///     The display name of the initiator.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    ///     The identifier of the initiator.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     The name of the initiator.
    /// </summary>
    public string Name { get; set; }
}
