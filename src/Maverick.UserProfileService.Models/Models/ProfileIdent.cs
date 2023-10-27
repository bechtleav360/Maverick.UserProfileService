using System.ComponentModel.DataAnnotations;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.EnumModels;

namespace Maverick.UserProfileService.Models.Models
{
    /// <summary>
    ///     Defines a profile by id and type
    /// </summary>
    public class ProfileIdent
    {
        /// <summary>
        ///     Unique identifier of profile
        /// </summary>
        [Required]
        [NotEmptyOrWhitespace]
        public string Id { get; set; }

        /// <summary>
        ///     Type of profile
        /// </summary>
        public ProfileKind ProfileKind { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="ProfileIdent"/>.
        /// </summary>
        public ProfileIdent()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ProfileIdent"/> with a specified id and profile kind.
        /// </summary>
        /// <param name="id">The unique identifier of the profile.</param>
        /// <param name="profileKind">The type of the profile.</param>
        public ProfileIdent(string id, ProfileKind profileKind)
        {
            Id = id;
            ProfileKind = profileKind;
        }
    }
}
