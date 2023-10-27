using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Maverick.UserProfileService.Models.Models;

namespace Maverick.UserProfileService.Models.RequestModels
{
    /// <summary>
    ///     The request to create a group.
    /// </summary>
    public class CreateGroupRequest
    {
        /// <summary>
        ///     The name for displaying
        /// </summary>
        public string DisplayName { set; get; }

        /// <summary>
        ///     A collection of ids that are used to identify the resource in an external source.
        /// </summary>
        public IList<ExternalIdentifier> ExternalIds { get; set; }

        /// <summary>
        ///     If true, the group is system-relevant, that means it will be treated as read-only.
        /// </summary>
        public bool IsSystem { set; get; }

        /// <summary>
        ///     The profile members of the group.
        /// </summary>
        public IList<ConditionAssignment> Members { get; set; } = new List<ConditionAssignment>();

        /// <summary>
        ///     The desired name of the group.
        /// </summary>
        [Required]
        public string Name { set; get; }

        /// <summary>
        ///     Tags to assign to group.
        /// </summary>
        public IList<TagAssignment> Tags { set; get; } = new List<TagAssignment>();

        /// <summary>
        ///     The weight can be used for weighting a group.
        /// </summary>
        public double Weight { set; get; } = 0;
    }
}
