using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Annotations;

namespace Maverick.UserProfileService.AggregateEvents.Resolved.V1
{
    /// <summary>
    ///     Defines an event emitted when tags of an object (i.e. profile, function, etc.) have ben deleted.<br />
    ///     Be aware! The version of this event does not correlate with the UPS API version.
    /// </summary>
    [AggregateEventDetails(true)]
    public class TagDeleted : IUserProfileServiceEvent
    {
        ///<inheridoc />
        public string EventId { get; set; }

        ///<inheridoc />
        public EventMetaData MetaData { get; set; } = new EventMetaData();

        /// <summary>
        ///     The tag id that should be deleted.
        /// </summary>
        public string TagId { get; set; }

        ///<inheridoc />
        public string Type => nameof(TagDeleted);
    }
}
