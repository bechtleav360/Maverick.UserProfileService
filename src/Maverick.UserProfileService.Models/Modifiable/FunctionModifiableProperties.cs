using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.BasicModels;

namespace Maverick.UserProfileService.Models.Modifiable
{
    /// <summary>
    ///     Contains all properties of a function that can be modified.
    /// </summary>
    public class FunctionModifiableProperties
    {
        /// <summary>
        ///     The Id of the organization <see cref="OrganizationBasic" />
        /// </summary>
        [NotEmptyOrWhitespace]
        public string OrganizationId { get; set; }

        /// <summary>
        ///     An unique string to identify the related role.
        /// </summary>
        [NotEmptyOrWhitespace]
        public string RoleId { set; get; }
    }
}
