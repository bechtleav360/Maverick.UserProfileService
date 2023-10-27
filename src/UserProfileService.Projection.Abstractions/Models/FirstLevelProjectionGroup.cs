using System;
using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.Models.Models;
using ProfileKind = Maverick.UserProfileService.Models.EnumModels.ProfileKind;

namespace UserProfileService.Projection.Abstractions.Models;

/// <summary>
///     The group that is used for the first level projection
///     repository.
/// </summary>
public class FirstLevelProjectionGroup : IFirstLevelProjectionContainer, IFirstLevelProjectionProfile
{
    /// <inheritdoc />
    public ContainerType ContainerType => ContainerType.Group;

    /// <inheritdoc />
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc />
    public string DisplayName { get; set; }

    /// <inheritdoc />
    public IList<ExternalIdentifier> ExternalIds { get; set; }

    /// <inheritdoc />
    public string Id { get; set; }

    /// <summary>
    ///     A boolean value that is true if the resource should be deleted but it is not possible cause of underlying
    ///     dependencies.
    /// </summary>
    public bool IsMarkedForDeletion { set; get; }

    /// <summary>
    ///     If true, the organization is system-relevant, that means it will be treated as read-only.
    /// </summary>
    public bool IsSystem { set; get; }

    /// <inheritdoc />
    public ProfileKind Kind => ProfileKind.Group;

    /// <inheritdoc />
    public string Name { get; set; }

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
