using System.Collections.Generic;
using Maverick.UserProfileService.Models.BasicModels;

namespace Maverick.UserProfileService.Models.Models
{
    /// <summary>
    ///     Group Model with relevant view properties
    /// </summary>
    public class GroupView : GroupBasic
    {
        /// <summary>
        ///     Is used to classify a group.
        /// </summary>
        public IList<string> Tags = new List<string>();

        /// <summary>
        ///     Amount of children.
        /// </summary>
        public int ChildrenCount { get; set; }

        /// <summary>
        ///     If the group has members / children
        /// </summary>
        public bool HasChildren { get; set; }
    }
}
