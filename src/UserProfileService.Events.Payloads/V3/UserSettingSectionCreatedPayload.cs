using Maverick.UserProfileService.Models.Annotations;

namespace UserProfileService.Events.Payloads.V3;

/// <summary>
///     The payload of a user-section-settings-created event.
/// </summary>
public class UserSettingSectionCreatedPayload : PayloadBase<UserSettingSectionCreatedPayload>
{
    /// <summary>
    ///     The id of the payload item.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     The name of the section that should be created.
    /// </summary>
    [NotEmptyOrWhitespace]
    public string SectionName { get; set; }

    /// <summary>
    ///     The user id related to the event.
    /// </summary>
    [NotEmptyOrWhitespace]
    public string UserId { get; set; }

    /// <summary>
    ///     The value of the user settings as JSON array string.
    /// </summary>
    public string ValuesAsJsonString { get; set; } = "[]";
}
