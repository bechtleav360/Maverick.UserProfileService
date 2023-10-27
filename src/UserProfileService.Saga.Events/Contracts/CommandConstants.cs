namespace UserProfileService.Saga.Events.Contracts;

/// <summary>
///     Defines all commands of saga messages.
/// </summary>
public static class CommandConstants
{
    /// <summary>
    ///     Command to create function.
    /// </summary>
    public const string FunctionCreate = "function-created";

    /// <summary>
    ///     Command to delete function.
    /// </summary>
    public const string FunctionDelete = "function-deleted";

    /// <summary>
    ///     Command to create group.
    /// </summary>
    public const string GroupCreate = "group-created";

    /// <summary>
    ///     Command to update assignments between objects.
    /// </summary>
    public const string ObjectAssignment = "object-assignment";

    /// <summary>
    ///     Command to create organization.
    /// </summary>
    public const string OrganizationCreate = "organization-created";

    /// <summary>
    ///     Command to change profile properties.
    /// </summary>
    public const string ProfileChange = "profile-properties-changed";

    /// <summary>
    ///     Command to delete profile.
    /// </summary>
    public const string ProfileDelete = "profile-deleted";

    /// <summary>
    ///     Command to change role properties.
    /// </summary>
    public const string RoleChange = "role-properties-changed";

    /// <summary>
    ///     Command to create group.
    /// </summary>
    public const string RoleCreate = "role-created";

    /// <summary>
    ///     Command to delete role.
    /// </summary>
    public const string RoleDelete = "role-deleted";

    /// <summary>
    ///     Command to create group.
    /// </summary>
    public const string UserCreate = "user-created";

    /// <summary>
    ///     Command to delete a single user setting object.
    /// </summary>
    public const string UserSettingObjectDeleted = "user-setting-object-deleted";

    /// <summary>
    ///     Command to set a user setting object as child of a section.
    /// </summary>
    public const string UserSettingObjectUpdated = "user-setting-object-updated";

    /// <summary>
    ///     Command to create a user setting section.
    /// </summary>
    public const string UserSettingSectionCreated = "user-setting-section-created";

    /// <summary>
    ///     Command to delete a complete user setting section.
    /// </summary>
    public const string UserSettingSectionDeleted = "user-setting-section-deleted";

    /// <summary>
    ///     Command to add tags to a certain function.
    /// </summary>
    public const string FunctionTagsAdded = "function-tags-added";

    /// <summary>
    ///     Command to remove tag from a certain function.
    /// </summary>
    public const string FunctionTagsRemoved = "function-tags-removed";

    /// <summary>
    ///     Command to delete a client settings from a profile
    /// </summary>
    public const string ProfileClientSettingsDeleted = "profile-client-settings-deleted";

    /// <summary>
    ///     Command to set a client settings batch to a profile.
    /// </summary>
    public const string ProfileClientSettingsSetBatch = "profile-client-settings-set-batch";

    /// <summary>
    ///     Command to set client settings to a profile.
    /// </summary>
    public const string ProfileClientSettingsSet = "profile-client-settings-set";

    /// <summary>
    ///     Command to update a client settings that is attached to a profile.
    /// </summary>
    public const string ProfileClientSettingsUpdated = "profile-client-update-set";

    /// <summary>
    ///     Command to delete a custom property that  is attached to a profile.
    /// </summary>
    public const string ProfileCustomPropertiesDeleted = "profile-custom-properties-deleted";

    /// <summary>
    ///     Command to set a custom property to a certain profile.
    /// </summary>
    public const string ProfileCustomPropertiesSet = "profile-custom-properties-set";

    /// <summary>
    ///     Command to remove an image that is attached to a profile.
    /// </summary>
    public const string ProfileImageRemoved = "profile-image-removed";

    /// <summary>
    ///     Command to upload an image to a certain profile.
    /// </summary>
    public const string ProfileImageUploaded = "profile-image-uploaded";

    /// <summary>
    ///     Command to add tags to a certain profile.
    /// </summary>
    public const string ProfileTagsAdded = "profile-tags-added";

    /// <summary>
    ///     Command to remove tags from a certain profile.
    /// </summary>
    public const string ProfileTagsRemoved = "profile-tags-removed";

    /// <summary>
    ///     Command to add tags to a certain role.
    /// </summary>
    public const string RoleTagsAdded = "role-tags-added";

    /// <summary>
    ///     Command to remove tags from a certain role.
    /// </summary>
    public const string RoleTagsRemoved = "role-tags-removed";

    /// <summary>
    ///     Command to create a tag.
    /// </summary>
    public const string TagCreated = "tag-created";

    /// <summary>
    ///     Command to delete a tag.
    /// </summary>
    public const string TagDeleted = "tag-deleted";
}

