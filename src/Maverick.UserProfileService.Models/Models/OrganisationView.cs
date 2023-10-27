using System.Collections.Generic;
using Maverick.UserProfileService.Models.BasicModels;

namespace Maverick.UserProfileService.Models.Models
{
    /// <summary>
    ///     Organization Model with relevant view properties
    /// </summary>
    public class OrganizationView : OrganizationBasic
    {
        /// <summary>
        ///     Is used to classify a organizations.
        /// </summary>
        public IList<string> Tags = new List<string>();

        /// <summary>
        ///     Amount of children.
        /// </summary>
        public int ChildrenCount { get; set; }

        /// <summary>
        ///     If the organization has members / children
        /// </summary>
        public bool HasChildren { get; set; }
    }
}
