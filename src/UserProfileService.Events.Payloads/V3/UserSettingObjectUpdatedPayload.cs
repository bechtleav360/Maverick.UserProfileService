using Maverick.UserProfileService.Models.Annotations;

namespace UserProfileService.Events.Payloads.V3;

/// <summary>
///     The payload of a user-setting-object-set event.
/// </summary>
public class UserSettingObjectUpdatedPayload : PayloadBase<UserSettingObjectUpdatedPayload>
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
    ///     The id of the setting object that should be modified.
    /// </summary>
    [NotEmptyOrWhitespace]
    public string SettingObjectId { get; set; }

    /// <summary>
    ///     The user id related to the event.
    /// </summary>
    [NotEmptyOrWhitespace]
    public string UserId { get; set; }

    /// <summary>
    ///     The value of the user setting object as JSON object string.
    /// </summary>
    public string ValuesAsJsonString { get; set; } = "{}";
}
