using System.ComponentModel.DataAnnotations;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.Models;

namespace Maverick.UserProfileService.Models.RequestModels
{
    /// <summary>
    ///     It is used to assign tags to entities.
    /// </summary>
    public class TagAssignment
    {
        /// <summary>
        ///     A boolean value that is true if the tag should be inherited.
        ///     For entities like <see cref="FunctionBasic" />, <see cref="RoleBasic" /> and <see cref="User" /> it will be
        ///     ignored.
        /// </summary>
        public bool IsInheritable { set; get; } = false;

        /// <summary>
        ///     The id representing the unique identifier of this tag
        /// </summary>
        [Required]
        [NotEmptyOrWhitespace]
        public string TagId { get; set; }
    }
}
