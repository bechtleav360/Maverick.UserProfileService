using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;

namespace UserProfileService.Events.Payloads.V2;

/// <summary>
///     Defines all properties required for creating a group.
///     Be aware, the versioning is related to the Payload and not to the API.
/// </summary>
public class GroupCreatedPayload : PayloadBase<GroupCreatedPayload>, ICreateModelPayload
{
    /// <summary>
    ///     The profile members of the group.
    /// </summary>
    public ConditionObjectIdent[] Members = Array.Empty<ConditionObjectIdent>();

    /// <summary>
    ///     The name that is used for displaying.
    /// </summary>
    [NotEmptyOrWhitespace]
    public string DisplayName { get; set; }

    /// <inheritdoc />
    public IList<ExternalIdentifier> ExternalIds { get; set; } = new List<ExternalIdentifier>();

    /// <summary>
    ///     Used to identify the group.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     If true, the group is system-relevant, that means it will be treated as read-only.
    /// </summary>
    public bool IsSystem { set; get; } = false;

    /// <summary>
    ///     Defines the name of the resource.
    /// </summary>
    [NotEmptyOrWhitespace]
    public string Name { get; set; }

    /// <summary>
    ///     The source name where the entity was transferred to (i.e. API, active directory).
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    ///     Tags to assign to group.
    /// </summary>
    public TagAssignment[] Tags { set; get; } = Array.Empty<TagAssignment>();

    /// <summary>
    ///     The weight of a group profile that can be used to sort a result set.
    /// </summary>
    public double Weight { set; get; } = 0;
}
