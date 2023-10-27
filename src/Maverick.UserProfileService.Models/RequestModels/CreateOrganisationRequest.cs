using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Maverick.UserProfileService.Models.Models;

namespace Maverick.UserProfileService.Models.RequestModels
{
    /// <summary>
    ///     The request to create a organization.
    /// </summary>
    public class CreateOrganizationRequest
    {
        /// <summary>
        ///     The profile members of the organization.
        /// </summary>
        public IList<ConditionAssignment> Members = new List<ConditionAssignment>();

        /// <summary>
        ///     The name for displaying
        /// </summary>
        public string DisplayName { set; get; }

        /// <summary>
        ///     A collection of ids that are used to identify the resource in an external source.
        /// </summary>
        public IList<ExternalIdentifier> ExternalIds { get; set; }

        /// <summary>
        ///     If true, the organization is system-relevant, that means it will be treated as read-only.
        /// </summary>
        public bool IsSystem { set; get; }

        /// <summary>
        ///     The desired name of the organization.
        /// </summary>
        [Required]
        public string Name { set; get; }

        /// <summary>
        ///     Tags to assign to organization.
        /// </summary>
        public IList<TagAssignment> Tags { set; get; } = new List<TagAssignment>();

        /// <summary>
        ///     The weight can be used for weighting a organization.
        /// </summary>
        public double Weight { set; get; } = 0;
    }
}
