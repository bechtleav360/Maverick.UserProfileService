using System;

namespace Maverick.UserProfileService.Models.Models
{
    /// <summary>
    ///     Represents the user settings section that store
    ///     a section with a name and an unique id.
    /// </summary>
    public class UserSettingSection
    {
        /// <summary>
        ///     The creation date of this section.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        ///     The id the section has in the database.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     The name of the section.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Creates an object of the type <see cref="UserSettingSection" />.
        /// </summary>
        public UserSettingSection()
        {
        }
    }
}
