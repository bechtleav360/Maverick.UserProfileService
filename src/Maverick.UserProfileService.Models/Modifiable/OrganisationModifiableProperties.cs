using Maverick.UserProfileService.Models.Annotations;

namespace Maverick.UserProfileService.Models.Modifiable
{
    /// <summary>
    ///     Contains all properties of a organization that can be modified.
    /// </summary>
    public class OrganizationModifiableProperties
    {
        /// <summary>
        ///     The name for displaying.
        /// </summary>
        [NotEmptyOrWhitespace]
        public string DisplayName { set; get; }

        /// <summary>
        ///     If true, the organization is system-relevant, that means it will be treated as read-only.
        /// </summary>
        public bool IsSystem { set; get; }

        /// <summary>
        ///     The name of the organization.
        ///     For example, used to generate names for the function.
        /// </summary>
        [NotEmptyOrWhitespace]
        public string Name { set; get; }

        /// <summary>
        ///     The weight can be used for weighting a organization.
        /// </summary>
        public double Weight { set; get; } = 0;
    }
}
