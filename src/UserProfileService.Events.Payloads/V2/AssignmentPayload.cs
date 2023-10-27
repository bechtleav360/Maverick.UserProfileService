using System;
using System.ComponentModel.DataAnnotations;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Events.Payloads.V2;

/// <summary>
///     A model wrapping all properties required for an assignment.
///     Be aware, the versioning is related to the Payload and not to the API.
/// </summary>
public class AssignmentPayload : PayloadBase<AssignmentPayload>
{
    /// <summary>
    ///     At least one id of a object to assign.
    /// </summary>
    public ConditionObjectIdent[] Added { get; set; } = Array.Empty<ConditionObjectIdent>();

    /// <summary>
    ///     At least one id of a object to unassign.
    /// </summary>
    public ConditionObjectIdent[] Removed { get; set; } = Array.Empty<ConditionObjectIdent>();

    /// <summary>
    ///     The id of the resource to assign the given objects to.
    /// </summary>
    [Required]
    [NotEmptyOrWhitespace]
    public ObjectIdent Resource { get; set; }

    /// <summary>
    ///     The type of the relation of the <see cref="Resource" /> to <see cref="Added" /> or <see cref="Removed" />.
    /// </summary>
    public AssignmentType Type { get; set; }
}
