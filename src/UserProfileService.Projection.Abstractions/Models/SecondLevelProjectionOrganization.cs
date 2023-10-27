using System;
using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using UserProfileService.Projection.Abstractions.Annotations;

namespace UserProfileService.Projection.Abstractions.Models;

public class SecondLevelProjectionOrganization : ISecondLevelProjectionContainer,
    ISecondLevelProjectionProfile
{
    /// <inheritdoc />
    [Readonly]
    public ContainerType ContainerType => ContainerType.Organization;

    /// <inheritdoc />
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc />
    public string DisplayName { get; set; }

    /// <inheritdoc />
    public IList<ExternalIdentifier> ExternalIds { get; set; } = new List<ExternalIdentifier>();

    /// <summary>
    ///     Used to identify the resource.
    /// </summary>
    [Readonly]
    public string Id { get; set; }

    /// <summary>
    ///     A boolean value that is true if the resource should be deleted but it is not possible cause of underlying
    ///     dependencies.
    /// </summary>
    public bool IsMarkedForDeletion { set; get; }

    /// <summary>
    ///     If true the organization is an sub-organization.
    /// </summary>
    public bool IsSubOrganization { get; set; }

    /// <summary>
    ///     If true, the organization is system-relevant, that means it will be treated as read-only.
    /// </summary>
    public bool IsSystem { set; get; }

    /// <inheritdoc />
    [Readonly]
    public ProfileKind Kind => ProfileKind.Organization;

    /// <inheritdoc />
    [Readonly]
    public IList<Member> MemberOf { get; set; }

    /// <inheritdoc />
    public string Name { get; set; }

    /// <summary>
    ///     A List containing the paths as string.
    /// </summary>
    [Readonly]
    public List<string> Paths { get; set; }

    /// <inheritdoc />
    public string Source { get; set; }

    /// <inheritdoc />
    public DateTime? SynchronizedAt { get; set; }

    /// <inheritdoc />
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    ///     The weight of a organization profile that can be used to sort a result set.
    /// </summary>
    public double Weight { set; get; } = 0;
}
