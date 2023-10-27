using System.ComponentModel.DataAnnotations;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.EnumModels;

namespace Maverick.UserProfileService.Models.RequestModels
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
        [Required]
        [NotEmptyOrWhitespace]
        [Searchable]
        public string Name { set; get; }

        /// <summary>
        ///     The type of the tag. For for information see <see cref="TagType" />.
        /// </summary>
        [Required]
        public TagType Type { get; set; } = TagType.Custom;
    }
}
