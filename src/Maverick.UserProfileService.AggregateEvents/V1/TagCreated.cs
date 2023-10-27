using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;

namespace Maverick.UserProfileService.AggregateEvents.V1
{
    /// <summary>
    ///     This event is emitted when a new tag is created.<br />
    ///     Be aware! The version of this event does not correlate with the UPS API version.
    /// </summary>
    public class TagCreated : IUserProfileServiceEvent
    {
        ///<inheridoc />
        public string EventId { get; set; }

        /// <summary>
        ///     The id representing the unique identifier of this tag.
        /// </summary>
        public string Id { get; set; }

        ///<inheridoc />
        public EventMetaData MetaData { get; set; } = new EventMetaData();

        /// <summary>
        ///     The name that will be used to tag or to classify the related resource.
        ///     The name is only used if no reference to an object is specified.
        /// </summary>
        public string Name { set; get; }

        /// <summary>
        ///     The type of the tag. For for information see <see cref="TagType" />.
        /// </summary>
        public TagType TagType { get; set; } = TagType.Custom;

        ///<inheridoc />
        public string Type => nameof(TagCreated);
    }
}
