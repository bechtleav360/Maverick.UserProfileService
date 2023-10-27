using Maverick.UserProfileService.Models.Annotations;

namespace UserProfileService.Events.Payloads.V3;

/// <summary>
///     The payload of a user-setting-section-deleted event.
/// </summary>
public class UserSettingSectionDeletedPayload : PayloadBase<UserSettingSectionDeletedPayload>
{
    /// <summary>
    ///     The id of the payload item.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     The name of the section that should be deleted.
    /// </summary>
    [NotEmptyOrWhitespace]
    public string SectionName { get; set; }

    /// <summary>
    ///     The user id related to the event.
    /// </summary>
    [NotEmptyOrWhitespace]
    public string UserId { get; set; }
}
