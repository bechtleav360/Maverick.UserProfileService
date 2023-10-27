using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;

namespace Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models
{
    /// <summary>
    ///     Short user object for the group list object.
    /// </summary>
    public class Member
    {
        /// <summary>
        ///     A list of range-condition settings valid for this <see cref="Member" />. If it is empty or <c>null</c>, the
        ///     membership of this <see cref="Member" /> is always active.
        /// </summary>
        public IList<RangeCondition> Conditions { get; set; }

        /// <summary>
        ///     The name that is used for displaying.
        /// </summary>
        public string DisplayName { set; get; }

        /// <summary>
        ///     A collection of ids that are used to identify the resource in an external source.
        /// </summary>
        public IList<ExternalIdentifier> ExternalIds { get; set; }

        /// <summary>
        ///     The identifier of the group member.
        /// </summary>
        public string Id { set; get; }

        /// <summary>
        ///     Determines if user, group or organization.
        /// </summary>
        public ProfileKind Kind { get; set; }

        /// <summary>
        ///     The name of the group member.
        /// </summary>
        public string Name { set; get; }
    }
}
