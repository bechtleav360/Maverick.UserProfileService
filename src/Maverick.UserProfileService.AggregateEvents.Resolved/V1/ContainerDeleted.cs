using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Annotations;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;

namespace Maverick.UserProfileService.AggregateEvents.Resolved.V1
{
    /// <summary>
    ///     Defines the event emitted when a container (or parent of an object) has been deleted.<br />
    ///     Be aware! The version of this event does not correlate with the UPS API version.
    /// </summary>
    [AggregateEventDetails(true)]
    public class ContainerDeleted : IUserProfileServiceEvent
    {
        /// <summary>
        ///     The id of the container that has been deleted.
        /// </summary>
        public string ContainerId { get; set; }

        /// <summary>
        ///     The container type of the current container.
        /// </summary>
        public ContainerType ContainerType { set; get; }

        ///<inheridoc />
        public string EventId { get; set; }

        /// <summary>
        ///     The id of the current entity whose container/parent has been deleted.
        /// </summary>
        public string MemberId { get; set; }

        ///<inheridoc />
        public EventMetaData MetaData { get; set; } = new EventMetaData();

        ///<inheridoc />
        public string Type => nameof(ContainerDeleted);
    }
}
