using System.Collections.Generic;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.EnumModels;

namespace Maverick.UserProfileService.Models.Models
{
    /// <summary>
    ///     Describes the linked function to an object.
    /// </summary>
    public class LinkedFunctionObject : ILinkedObject
    {
        /// <summary>
        ///     A list of range-condition settings valid for this <see cref="LinkedFunctionObject" />. If it is empty or
        ///     <c>null</c>, the membership of this <see cref="LinkedFunctionObject" /> is always active.
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
        ///     The organization id to identify the related organization,
        ///     that is used for the function.
        /// </summary>
        public string OrganizationId { get; set; }

        /// <summary>
        ///     The name of the organization of the current function.
        /// </summary>
        public string OrganizationName { get; set; }

        /// <summary>
        ///     The id of the role of the current
        /// </summary>
        public string RoleId { get; set; }

        /// <summary>
        ///     The role name of the function.
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        ///     Identifies the type of the linked object. In this case it is "function".
        /// </summary>
        public string Type { set; get; } = RoleType.Function.ToString();
    }
}
