using System;
using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;

namespace UserProfileService.Projection.Abstractions;

/// <summary>
///     The interface marks all profiles that are used for the second level projection repository.
/// </summary>
public interface ISecondLevelProjectionProfile
{
    /// <summary>
    ///     The time when the resource has been created.
    /// </summary>
    DateTime CreatedAt { set; get; }

    /// <summary>
    ///     The name that is used for displaying.
    /// </summary>
    string DisplayName { set; get; }

    /// <summary>
    ///     A collection of ids that are used to identify the resource in an external source.
    /// </summary>
    IList<ExternalIdentifier> ExternalIds { get; set; }

    /// <summary>
    ///     Used to identify the resource.
    /// </summary>
    string Id { get; }

    /// <summary>
    ///     A profile kind is used to identify a profile.
    /// </summary>
    ProfileKind Kind { get; }

    /// <summary>
    ///     Assignment status of a user.
    /// </summary>
    IList<Member> MemberOf { set; get; }

    /// <summary>
    ///     Defines the name of the resource.
    /// </summary>
    string Name { set; get; }

    /// <summary>
    ///     A List containing the paths as string.
    /// </summary>
    public List<string> Paths { get; set; }

    /// <summary>
    ///     The key of the source system the entity is synced from.
    /// </summary>
    string Source { get; set; }

    /// <summary>
    ///     The time stamp when the object has been synchronized the last time.
    /// </summary>
    DateTime? SynchronizedAt { set; get; }

    /// <summary>
    ///     The time when the resource has been updated lastly.
    /// </summary>
    DateTime UpdatedAt { set; get; }
}
