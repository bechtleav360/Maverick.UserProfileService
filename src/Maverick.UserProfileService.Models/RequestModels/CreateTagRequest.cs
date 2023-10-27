using System.ComponentModel.DataAnnotations;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.EnumModels;

namespace Maverick.UserProfileService.Models.RequestModels
{
    /// <summary>
    ///     The request to create a tag.
    /// </summary>
    public class CreateTagRequest
    {
        /// <summary>
        ///     The name that will be used to tag or to classify the related resource.
        /// </summary>
        [Required]
        [NotEmptyOrWhitespace]
        public string Name { set; get; }

        /// <summary>
        ///     The type of the tag. For for information see <see cref="TagType" />.
        /// </summary>
        [Required]
        public TagType Type { get; set; } = TagType.Custom;
    }
}
