using System.ComponentModel.DataAnnotations;
using Maverick.UserProfileService.Models.Annotations;

namespace UserProfileService.Events.Payloads.V2;

/// <summary>
///     A model wrapping all properties necessary to set the parent of an object.
///     Be aware, the versioning is related to the Payload and not to the API.
/// </summary>
public class ParentSetPayload : PayloadBase<ParentSetPayload>
{
    /// <summary>
    ///     Specifies the new Parent.
    /// </summary>
    [Required]
    [NotEmptyOrWhitespace]
    public string ParentId { get; set; }

    /// <summary>
    ///     Identifies one or more objects to set the parent of.
    /// </summary>
    [Required]
    [MinLength(1)]
    public string[] SecOIds { get; set; }
}
