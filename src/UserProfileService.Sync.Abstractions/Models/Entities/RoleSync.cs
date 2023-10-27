using System.Collections.Generic;
using UserProfileService.Sync.Abstraction.Annotations;
using UserProfileService.Sync.Abstraction.Contracts;

namespace UserProfileService.Sync.Abstraction.Models.Entities;

/// <summary>
///     The implementation of <see cref="ISyncModel" /> for roles.
/// </summary>
[Model(SyncConstants.Models.Role)]
public class RoleSync : ISyncModel
{
    /// <summary>
    ///     Contains term to reject or denied rights.
    /// </summary>
    public IList<string> DeniedPermissions { set; get; } = new List<string>();

    /// <summary>
    ///     A statement describing the role.
    /// </summary>
    public string Description { set; get; }

    /// <inheritdoc />
    public IList<KeyProperties> ExternalIds { get; set; } = new List<KeyProperties>();

    /// <inheritdoc />
    public string Id { get; set; }

    /// <summary>
    ///     If true, the group is system-relevant, that means it will be treated as read-only.
    /// </summary>
    public bool IsSystem { set; get; }

    /// <summary>
    ///     Defines the name of the role.
    /// </summary>
    public string Name { set; get; }

    /// <summary>
    ///     Contains terms to authorize or grant rights.
    /// </summary>
    public IList<string> Permissions { set; get; } = new List<string>();

    /// <inheritdoc />
    public List<ObjectRelation> RelatedObjects { get; set; } = new List<ObjectRelation>();

    /// <summary>
    ///     The source name where the entity was transferred from (i.e. API, active directory).
    /// </summary>
    public string Source { get; set; }
}
