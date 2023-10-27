using System.ComponentModel.DataAnnotations;

namespace UserProfileService.Events.Payloads.V2;

/// <summary>
///     Defines a payload for identifier usage.
///     Be aware, the versioning is related to the Payload and not to the API.
/// </summary>
public class IdentifierCollectionPayload : PayloadBase<IdentifierCollectionPayload>
{
    /// <summary>
    ///     Collection of identifier of entities to processed in saga message.
    /// </summary>
    [Required]
    [MinLength(1)]
    public string[] Ids { get; set; }
}
