using System.Collections.Generic;
using Maverick.UserProfileService.Models.Models;

namespace Maverick.UserProfileService.Models.Abstraction
{
    /// <summary>
    ///     An abstraction for an linked object (role or function)
    /// </summary>
    public interface ILinkedObject
    {
        /// <summary>
        ///     A list of range-condition settings valid for this <see cref="ILinkedObject" />. If it is empty or <c>null</c>, the
        ///     membership of this <see cref="ILinkedObject" /> is always active.
        /// </summary>
        List<RangeCondition> Conditions { get; set; }

        /// <summary>
        ///     Used to identify the object.
        /// </summary>
        string Id { set; get; }

        /// <summary>
        ///     Determines if any condition of the list is currently active.
        /// </summary>
        bool IsActive { get; set; }

        /// <summary>
        ///     Defines the name of the object.
        /// </summary>
        string Name { set; get; }

        /// <summary>
        ///     Identifies the type of the linked object. It can be a role or function.
        /// </summary>
        string Type { set; get; }
    }
}
