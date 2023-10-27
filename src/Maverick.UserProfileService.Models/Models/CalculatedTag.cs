using Maverick.UserProfileService.Models.EnumModels;

namespace Maverick.UserProfileService.Models.Models
{
    /// <summary>
    ///     Is used to classify a profile, a function, a role or an object.
    /// </summary>
    public class CalculatedTag
    {
        /// <summary>
        ///     The id representing the unique identifier of this tag.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     A boolean value that is true if the tag was inherited by an related resource object.
        /// </summary>
        public bool IsInherited { set; get; } = false;

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
