using System.Text.Json.Nodes;

namespace UserProfileService.Adapter.Marten.EntityModels;

/// <summary>
///     Represent the object that  stores an object for a certain user and
///     a certain section.The model is used to store the information in the database.
/// </summary>
public class UserSettingObjectDbModel
{
    /// <summary>
    ///     The date when the client settings Object was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    ///     The id of the client settings object.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    ///     The date when the client settings object was the last time updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    /// <summary>
    ///     The id of the user the object belongs to.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    ///     The key of the user client setting.
    /// </summary>
    public UserSettingSectionDbModel UserSettingSection { set; get; } = new UserSettingSectionDbModel();

    /// <summary>
    ///     The client settings object that stores an json object.
    /// </summary>
    public JsonObject UserSettingsObject { get; set; }

    /// <summary>
    ///     Create an object of type <see cref="UserSettingObjectDbModel" />.
    /// </summary>
    /// <param name="userSettingsObject">The settings object that is stored for a user.</param>
    /// <param name="userSettingSection">The section for the object.</param>
    /// <param name="userId">The user that contains the object.</param>
    public UserSettingObjectDbModel(
        JsonObject userSettingsObject,
        UserSettingSectionDbModel userSettingSection,
        string userId)
    {
        UserSettingsObject = userSettingsObject;
        UserSettingSection = userSettingSection;
        UserId = userId;
    }

    /// <summary>
    ///     Default Constructor to create the object of type <see cref="UserSettingObjectDbModel" />.
    ///     The default constructor is needed to deserialize the object from the database.
    /// </summary>
    public UserSettingObjectDbModel()
    {
        UserSettingsObject = new JsonObject();
    }

    /// <summary>
    ///     A copy constructor to copy an <see cref="UserSettingsObject" />.
    /// </summary>
    /// <param name="copyObject">The object that should be copied.</param>
    public UserSettingObjectDbModel(UserSettingObjectDbModel copyObject)
    {
        Id = copyObject.Id;
        CreatedAt = copyObject.CreatedAt;
        UpdatedAt = copyObject.UpdatedAt;
        UserSettingSection = copyObject.UserSettingSection;
        UserId = copyObject.UserId;

        UserSettingsObject = JsonNode.Parse(copyObject.UserSettingsObject.ToJsonString())?.AsObject()
            ?? new JsonObject();
    }
}
