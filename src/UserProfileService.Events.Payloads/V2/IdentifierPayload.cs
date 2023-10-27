using System.ComponentModel.DataAnnotations;
using Maverick.UserProfileService.Models.Annotations;

namespace UserProfileService.Events.Payloads.V2;

/// <summary>
///     Defines a payload for identifier usage.
///     Be aware, the versioning is related to the Payload and not to the API.
/// </summary>
public class IdentifierPayload : PayloadBase<IdentifierPayload>
{
    /// <summary>
    ///     Identifier of entity to processed in saga message.
    /// </summary>
    [Required]
    [NotEmptyOrWhitespace]
    public string Id { get; set; }
}
