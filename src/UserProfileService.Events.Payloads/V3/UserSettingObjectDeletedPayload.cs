using Maverick.UserProfileService.Models.Annotations;

namespace UserProfileService.Events.Payloads.V3;

/// <summary>
///     The payload of a user-setting-object-deleted event.
/// </summary>
public class UserSettingObjectDeletedPayload : PayloadBase<UserSettingObjectDeletedPayload>
{
    /// <summary>
    ///     The id of the payload item.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     The name of the section whose child should be deleted.
    /// </summary>
    [NotEmptyOrWhitespace]
    public string SectionName { get; set; }

    /// <summary>
    ///     The id of the setting object that should be deleted.
    /// </summary>
    [NotEmptyOrWhitespace]
    public string SettingObjectId { get; set; }

    /// <summary>
    ///     The user id related to the event.
    /// </summary>
    [NotEmptyOrWhitespace]
    public string UserId { get; set; }
}
