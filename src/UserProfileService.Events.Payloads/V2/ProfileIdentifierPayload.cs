using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Events.Payloads.V2;

/// <summary>
///     Defines a payload for identifier usage.
///     Be aware, the versioning is related to the Payload and not to the API.
/// </summary>
public class ProfileIdentifierPayload : PayloadBase<ProfileIdentifierPayload>
{
    /// <summary>
    ///     A collection of ids that are used to identify the resource in an external source.
    /// </summary>
    public IList<ExternalIdentifier> ExternalIds { get; set; } = new List<ExternalIdentifier>();

    /// <summary>
    ///     Identifier of entity to processed in saga message.
    /// </summary>
    [Required]
    [NotEmptyOrWhitespace]
    public string Id { get; set; }

    /// <summary>
    ///     Profile kind of current entity.
    /// </summary>
    [Required]
    public ProfileKind ProfileKind { get; set; }
}
