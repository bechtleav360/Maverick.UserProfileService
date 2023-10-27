using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Projection.Abstractions;

/// <summary>
///     The interface marks all profiles that are used for the first level projection repository.
/// </summary>
public interface IFirstLevelProjectionProfile : IFirstLevelProjectionSimplifier
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
    ///     The id representing the unique identifier of this object.
    /// </summary>
    string Id { get; set; }

    /// <summary>
    ///     A profile kind is used to identify a profile. Either it is group or a user.
    /// </summary>
    ProfileKind Kind { get; }

    /// <summary>
    ///     Defines the name of the resource.
    /// </summary>
    string Name { set; get; }

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
