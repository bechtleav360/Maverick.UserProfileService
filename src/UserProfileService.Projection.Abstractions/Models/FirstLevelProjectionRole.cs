using System;
using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Projection.Abstractions.Models;

/// <summary>
///     The container role that is used for the first level projection
///     repository.
/// </summary>
public class FirstLevelProjectionRole : IFirstLevelProjectionContainer, IFirstLevelProjectionSimplifier
{
    /// <inheritdoc />
    public ContainerType ContainerType => ContainerType.Role;

    /// <summary>
    ///     The time when the role has been created.
    /// </summary>
    public DateTime CreatedAt { set; get; }

    /// <summary>
    ///     Contains terms to reject or denied rights.
    /// </summary>
    public IList<string> DeniedPermissions { set; get; } = new List<string>();

    /// <summary>
    ///     A statement describing the role.
    /// </summary>
    public string Description { set; get; }

    /// <summary>
    ///     A collection of ids that are used to identify the resource in an external source.
    /// </summary>
    public IList<ExternalIdentifier> ExternalIds { get; set; }

    /// <summary>
    ///     The id of the role.
    /// </summary>
    public string Id { set; get; }

    /// <summary>
    ///     If true, the group is system-relevant, that means it will be treated as read-only.
    /// </summary>
    public bool IsSystem { set; get; } = false;

    /// <summary>
    ///     Defines the name of the role.
    /// </summary>
    public string Name { set; get; }

    /// <summary>
    ///     Contains terms to authorize or grant rights.
    /// </summary>
    public IList<string> Permissions { set; get; } = new List<string>();

    /// <summary>
    ///     The source name where the entity was transferred to (i.e. API, active directory).
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    ///     The time stamp when the object has been synchronized the last time.
    /// </summary>
    public DateTime? SynchronizedAt { set; get; }

    /// <summary>
    ///     The time when the role has been updated lastly.
    /// </summary>
    public DateTime UpdatedAt { set; get; }
}
