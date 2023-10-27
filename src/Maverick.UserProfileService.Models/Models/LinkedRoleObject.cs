using System.Collections.Generic;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.EnumModels;

namespace Maverick.UserProfileService.Models.Models
{
    /// <summary>
    ///     Describes the linked role to an object.
    /// </summary>
    public class LinkedRoleObject : ILinkedObject
    {
        /// <summary>
        ///     A list of range-condition settings valid for this <see cref="Member" />. If it is empty or <c>null</c>, the
        ///     membership of this <see cref="Member" /> is always active.
        /// </summary>
        public List<RangeCondition> Conditions { get; set; }

        /// <summary>
        ///     Used to identify the object.
        /// </summary>
        public string Id { set; get; }

        /// <summary>
        ///     Determines if any condition of the list is currently active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        ///     Defines the name of the object.
        /// </summary>
        public string Name { set; get; }

        /// <summary>
        ///     Identifies the type of the linked object. In this case it is "role".
        /// </summary>
        public string Type { set; get; } = RoleType.Role.ToString();
    }
}
