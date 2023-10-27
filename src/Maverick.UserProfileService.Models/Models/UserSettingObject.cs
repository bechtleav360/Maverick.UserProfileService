using System;
using System.Text.Json.Nodes;

namespace Maverick.UserProfileService.Models.Models
{
    /// <summary>
    ///     Represents the client settings user object that
    ///     contains a key and a json object.
    /// </summary>
    public class UserSettingObject
    {
        /// <summary>
        ///     The date when the client settings Object was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        ///     The id of the client settings object.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     The date when the client settings object was the last time updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        ///     The id of the user the object belongs to.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        ///     The key of the user client setting.
        /// </summary>
        public UserSettingSection UserSettingSection { set; get; }

        /// <summary>
        ///     The client settings object that stores a json object.
        /// </summary>
        public JsonObject UserSettingsObject { get; set; }

        /// <summary>
        ///     Creates an object of the type <see cref="UserSettingObject" />.
        /// </summary>
        public UserSettingObject()
        {
        }
    }
}
