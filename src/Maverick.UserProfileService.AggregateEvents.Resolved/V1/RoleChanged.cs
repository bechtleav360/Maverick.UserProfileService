using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Annotations;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;

namespace Maverick.UserProfileService.AggregateEvents.Resolved.V1
{
    /// <summary>
    ///     Defines the event emitted when the role has been changed.<br />
    ///     Be aware! The version of this event does not correlate with the UPS API version.
    /// </summary>
    [AggregateEventDetails(true)]
    public class RoleChanged : IUserProfileServiceEvent
    {
        /// <summary>
        ///     The context defines which members have to be changed due to
        ///     the role change.
        /// </summary>
        public PropertiesChangedContext Context { set; get; }

        ///<inheridoc />
        public string EventId { get; set; }

        ///<inheridoc />
        public EventMetaData MetaData { get; set; }

        /// <summary>
        ///     The role that is needed to update the role in a member.
        /// </summary>
        public Role Role { get; set; }

        ///<inheridoc />
        public string Type { get; set; }
    }
}
