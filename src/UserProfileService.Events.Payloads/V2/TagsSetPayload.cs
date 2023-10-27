using System.ComponentModel.DataAnnotations;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.RequestModels;

namespace UserProfileService.Events.Payloads.V2;

/// <summary>
///     A model wrapping all properties required for setting tags.
///     Be aware, the versioning is related to the Payload and not to the API.
/// </summary>
public class TagsSetPayload : PayloadBase<TagsSetPayload>
{
    /// <summary>
    ///     Identifies the resource.
    /// </summary>
    [Required]
    [NotEmptyOrWhitespace]
    public string Id { get; set; }

    /// <summary>
    ///     A boolean value that is true if the tags should be inherited.
    /// </summary>
    [Required]
    [MinLength(1)]
    public TagAssignment[] Tags { get; set; }
}
