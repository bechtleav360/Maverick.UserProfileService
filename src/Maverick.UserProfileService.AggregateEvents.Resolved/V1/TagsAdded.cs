using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;

namespace Maverick.UserProfileService.AggregateEvents.Resolved.V1
{
    /// <summary>
    ///     Defines an event emitted when a tag has been added to an object/resource (i.e profile, function, etc.).<br />
    ///     Be aware! The version of this event does not correlate with the UPS API version.
    /// </summary>
    public class TagsAdded : IUserProfileServiceEvent
    {
        ///<inheridoc />
        public string EventId { get; set; }

        /// <summary>
        ///     Identifies the resource.
        /// </summary>
        public string Id { get; set; }

        ///<inheridoc />
        public EventMetaData MetaData { get; set; } = new EventMetaData();

        /// <summary>
        ///     The type of the resource.
        /// </summary>
        public ObjectType ObjectType { get; set; }

        /// <summary>
        ///     A boolean value that is true if the tags should be inherited.
        /// </summary>
        public TagAssignment[] Tags { get; set; }

        ///<inheridoc />
        public string Type => nameof(TagsAdded);
    }
}
