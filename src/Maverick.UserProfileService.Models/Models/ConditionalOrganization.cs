using System.Collections.Generic;

namespace Maverick.UserProfileService.Models.Models
{
    /// <summary>
    ///     The view model of an organization plus a collection of <see cref="RangeCondition" />s.
    /// </summary>
    public class ConditionalOrganization : OrganizationView
    {
        /// <summary>
        ///     A list of range-condition settings valid for this <see cref="Member" />. If it is empty or <c>null</c>, the
        ///     membership of this <see cref="Member" /> is always active.
        /// </summary>
        public IList<RangeCondition> Conditions { get; set; }

        /// <summary>
        ///     Determines if any condition of the list is currently active.
        /// </summary>
        public bool IsActive { get; set; }
    }
}
