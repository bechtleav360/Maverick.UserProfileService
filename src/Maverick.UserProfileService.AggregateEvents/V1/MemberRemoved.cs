using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;

namespace Maverick.UserProfileService.AggregateEvents.V1
{
    /// <summary>
    ///     Defines the event emitted when a child has been
    ///     unassigned from the parent.
    /// </summary>
    public class MemberRemoved : IUserProfileServiceEvent
    {
        ///<inheridoc />
        public string EventId { get; set; }

        /// <summary>
        ///     The id of the member that has been unassigned.
        /// </summary>
        public string MemberId { get; set; }

        /// <summary>
        ///     The profile kind of the member that has been unassigned.
        /// </summary>
        public ProfileKind MemberKind { set; get; }

        ///<inheridoc />
        public EventMetaData MetaData { get; set; } = new EventMetaData();

        /// <summary>
        ///     The id of the current parent.
        /// </summary>
        public string ParentId { get; set; }

        /// <summary>
        ///     The container type of the current parent.
        /// </summary>
        public ContainerType ParentType { set; get; }

        ///<inheridoc />
        public string Type => nameof(MemberRemoved);
    }
}
