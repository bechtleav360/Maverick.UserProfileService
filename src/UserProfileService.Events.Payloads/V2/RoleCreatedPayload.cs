using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;

namespace UserProfileService.Events.Payloads.V2;

/// <summary>
///     A model used to wrap all properties required for creating a role.
///     Be aware, the versioning is related to the Payload and not to the API.
/// </summary>
public class RoleCreatedPayload : PayloadBase<RoleCreatedPayload>, ICreateModelPayload
{
    /// <summary>
    ///     Contains term to reject or denied rights.
    /// </summary>
    public IList<string> DeniedPermissions { set; get; } = new List<string>();

    /// <summary>
    ///     A statement describing the role.
    /// </summary>
    [Required]
    [NotEmptyOrWhitespace]
    public string Description { set; get; }

    /// <inheritdoc />
    public IList<ExternalIdentifier> ExternalIds { get; set; } = new List<ExternalIdentifier>();

    /// <summary>
    ///     Used to identify the role.
    /// </summary>
    [Required]
    [NotEmptyOrWhitespace]
    public string Id { set; get; }

    /// <summary>
    ///     If true, the group is system-relevant, that means it will be treated as read-only.
    /// </summary>
    public bool IsSystem { set; get; } = false;

    /// <summary>
    ///     Defines the name of the role.
    /// </summary>
    [Required]
    [NotEmptyOrWhitespace]
    public string Name { set; get; }

    /// <summary>
    ///     Contains terms to authorize or grant rights.
    /// </summary>
    public IList<string> Permissions { set; get; } = new List<string>();

    /// <summary>
    ///     The source name where the entity was transferred from (i.e. API, active directory).
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    ///     Tags to assign to group.
    /// </summary>
    public TagAssignment[] Tags { set; get; } = Array.Empty<TagAssignment>();
}
