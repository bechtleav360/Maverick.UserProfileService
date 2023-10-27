using Maverick.UserProfileService.AggregateEvents.Common.Enums;

namespace Maverick.UserProfileService.AggregateEvents.Common.Models
{
    /// <summary>
    ///     Is used to classify a profile, a function, a role or an object.
    /// </summary>
    public class Tag
    {
        /// <summary>
        ///     The id representing the unique identifier of this tag.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     The name that will be used to tag or to classify the related resource.
        /// </summary>
        public string Name { set; get; }

        /// <summary>
        ///     The type of the tag. For for information see <see cref="TagType" />.
        /// </summary>
        public TagType Type { get; set; } = TagType.Custom;
    }
}
