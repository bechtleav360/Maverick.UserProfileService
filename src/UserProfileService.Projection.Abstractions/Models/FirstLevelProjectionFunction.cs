using System;
using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Projection.Abstractions.Models;

/// <summary>
///     The container function that is used for the first level projection
///     repository.
/// </summary>
public class FirstLevelProjectionFunction : IFirstLevelProjectionContainer, IFirstLevelProjectionSimplifier
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
    public IList<ExternalIdentifier> ExternalIds { get; set; }

    /// <summary>
    ///     A unique string to identify a function.
    /// </summary>
    public string Id { set; get; }

    /// <summary>
    ///     The base model of a organization.
    /// </summary>
    public FirstLevelProjectionOrganization Organization { get; set; }

    /// <summary>
    ///     Describes the role that is related to the function.
    /// </summary>
    public FirstLevelProjectionRole Role { set; get; }

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
