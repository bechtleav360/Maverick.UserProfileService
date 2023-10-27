using System;
using System.ComponentModel.DataAnnotations;
using Maverick.UserProfileService.Models.Annotations;

namespace UserProfileService.Events.Payloads.V2;

/// <summary>
///     A model wrapping all properties required for deleting tags.
///     Be aware, the versioning is related to the Payload and not to the API.
/// </summary>
public class TagsRemovedPayload : PayloadBase<TagsRemovedPayload>
{
    /// <summary>
    ///     Identifies the resource.
    /// </summary>
    [Required]
    [NotEmptyOrWhitespace]
    public string ResourceId { get; set; }

    /// <summary>
    ///     Contains all identifier of the tags to remove.
    /// </summary>
    [Required]
    [MinLength(1)]
    public string[] Tags { get; set; } = Array.Empty<string>();
}
