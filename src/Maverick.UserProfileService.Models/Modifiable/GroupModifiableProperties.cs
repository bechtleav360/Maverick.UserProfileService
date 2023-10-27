using Maverick.UserProfileService.Models.Annotations;

namespace Maverick.UserProfileService.Models.Modifiable
{
    /// <summary>
    ///     Contains all properties of a group that can be modified.
    /// </summary>
    public class GroupModifiableProperties
    {
        /// <summary>
        ///     The name for displaying.
        /// </summary>
        [NotEmptyOrWhitespace]
        public string DisplayName { set; get; }

        /// <summary>
        ///     If true, the group is system-relevant, that means it will be treated as read-only.
        /// </summary>
        public bool IsSystem { set; get; }

        /// <summary>
        ///     The weight can be used for weighting a group.
        /// </summary>
        public double Weight { set; get; } = 0;
    }
}
