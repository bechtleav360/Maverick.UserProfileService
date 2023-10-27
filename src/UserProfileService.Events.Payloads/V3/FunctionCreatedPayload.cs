using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;

namespace UserProfileService.Events.Payloads.V3;

/// <summary>
///     Defines a model wrapping all properties required for creating function.
///     Be aware, the versioning is related to the Payload and not to the API.
/// </summary>
public class FunctionCreatedPayload : PayloadBase<FunctionCreatedPayload>, ICreateModelPayload
{
    /// <inheritdoc />
    public IList<ExternalIdentifier> ExternalIds { get; set; } = new List<ExternalIdentifier>();

    /// <summary>
    ///     A unique string to identify a function.
    /// </summary>
    [NotEmptyOrWhitespace]
    public string Id { set; get; }

    /// <summary>
    ///     Defines the name of the resource.
    /// </summary>
    [NotEmptyOrWhitespace]
    public string Name { set; get; }

    /// <summary>
    ///     The Id of the organization.
    /// </summary>
    [NotEmptyOrWhitespace]
    public string OrganizationId { get; set; }

    /// <summary>
    ///     A string to identify the role linked with this function.
    /// </summary>
    [NotEmptyOrWhitespace]
    public string RoleId { set; get; }

    /// <summary>
    ///     The source name where the entity was transferred to (i.e. API, active directory).
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    ///     Tags to assign to group.
    /// </summary>
    public TagAssignment[] Tags { set; get; } = Array.Empty<TagAssignment>();
}
