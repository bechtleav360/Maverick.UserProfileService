using System.Collections.Generic;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Sync.Abstraction.Annotations;
using UserProfileService.Sync.Abstraction.Contracts;

namespace UserProfileService.Sync.Abstraction.Models.Entities;

/// <summary>
///     The implementation of <see cref="ISyncProfile" /> for groups.
/// </summary>
[Model(SyncConstants.Models.Group)]
public class GroupSync : ISyncProfile
{
    /// <summary>
    ///     The name for displaying
    /// </summary>
    public string DisplayName { set; get; }

    /// <inheritdoc />
    public IList<KeyProperties> ExternalIds { get; set; } = new List<KeyProperties>();

    /// <inheritdoc />
    public string Id { get; set; }

    /// <summary>
    ///     If true, the group is system-relevant, that means it will be treated as read-only.
    /// </summary>
    public bool IsSystem { set; get; }

    /// <inheritdoc cref="ISyncProfile.Kind" />
    public ProfileKind Kind { get; set; } = ProfileKind.Group;

    /// <summary>
    ///     The desired name of the group.
    /// </summary>
    public string Name { get; set; }

    /// <inheritdoc />
    public List<ObjectRelation> RelatedObjects { get; set; } = new List<ObjectRelation>();

    /// <summary>
    ///     The source name where the entity was transferred from (i.e. API, active directory).
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    ///     The tags of the group.
    /// </summary>
    public List<CalculatedTag> Tags { get; set; } = new List<CalculatedTag>();

    /// <summary>
    ///     The weight can be used for weighting a group.
    /// </summary>
    public double Weight { set; get; }
}
