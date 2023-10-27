using System;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;

namespace Maverick.UserProfileService.AggregateEvents.Resolved.V1
{
    /// <summary>
    ///     Defines an event emitted when tags of an object (i.e. profile, function, etc.) have ben removed.<br />
    ///     Be aware! The version of this event does not correlate with the UPS API version.
    /// </summary>
    public class TagsRemoved : IUserProfileServiceEvent
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
        ///     Contains all identifier of the deleted tags.
        /// </summary>
        public string[] Tags { get; set; } = Array.Empty<string>();

        ///<inheridoc />
        public string Type => nameof(TagsRemoved);
    }
}
