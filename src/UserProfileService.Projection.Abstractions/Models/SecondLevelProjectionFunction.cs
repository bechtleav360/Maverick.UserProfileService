using System;
using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using UserProfileService.Projection.Abstractions.Annotations;

namespace UserProfileService.Projection.Abstractions.Models;

public class SecondLevelProjectionFunction : ISecondLevelProjectionContainer
{
    /// <inheritdoc />
    public ContainerType ContainerType => ContainerType.Function;

    /// <summary>
    ///     The time when the resource has been created.
    /// </summary>
    public DateTime CreatedAt { set; get; }

    /// <summary>
    ///     A collection of ids that are used to identify the resource in an external source.
    /// </summary>
    public IList<ExternalIdentifier> ExternalIds { get; set; } = new List<ExternalIdentifier>();

    /// <summary>
    ///     A unique string to identify a function.
    /// </summary>
    [Readonly]
    public string Id { set; get; }

    /// <summary>
    ///     The base model of a organization.
    /// </summary>
    public Organization Organization { get; set; }

    /// <summary>
    ///     The Id of the organization <see cref="Organization" />.
    /// </summary>
    [Readonly]
    public string OrganizationId { get; set; }

    /// <summary>
    ///     Describes the role that is related to the function.
    /// </summary>
    public Role Role { set; get; }

    /// <summary>
    ///     The id of the role that is related to the function.
    ///     <br />
    ///     Is required to validate the assignment of the role during updating.
    /// </summary>
    [Readonly]
    public string RoleId { get; set; }

    /// <summary>
    ///     The source name where the entity was transferred to (i.e. API, active directory).
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    ///     The time stamp when the object has been synchronized the last time.
    /// </summary>
    public DateTime? SynchronizedAt { set; get; }

    /// <summary>
    ///     The time when the resource has been updated lastly.
    /// </summary>
    public DateTime UpdatedAt { set; get; }
}
