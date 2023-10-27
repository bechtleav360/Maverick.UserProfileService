using System.Collections.Generic;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Sync.Abstraction.Annotations;
using UserProfileService.Sync.Abstraction.Contracts;

namespace UserProfileService.Sync.Abstraction.Models.Entities;

/// <summary>
///     The implementation of <see cref="ISyncModel" /> for organizations.
/// </summary>
[Model(SyncConstants.Models.Organization)]
public class OrganizationSync : ISyncProfile
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
    ///     If true the organization is an sub-organization.
    /// </summary>
    public bool IsSubOrganization { get; set; }

    /// <summary>
    ///     If true, the organization is system-relevant, that means it will be treated as read-only.
    /// </summary>
    public bool IsSystem { set; get; }

    /// <inheritdoc cref="ISyncProfile.Kind" />
    public ProfileKind Kind { get; set; } = ProfileKind.Organization;

    /// <summary>
    ///     The desired name of the organization.
    /// </summary>
    public string Name { get; set; }

    /// <inheritdoc />
    public List<ObjectRelation> RelatedObjects { get; set; }

    /// <summary>
    ///     The source name where the entity was transferred from (i.e. API, active directory).
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    ///     The tags of the organization.
    /// </summary>
    public List<CalculatedTag> Tags { get; set; } = new List<CalculatedTag>();

    /// <summary>
    ///     The weight can be used for weighting a organization.
    /// </summary>
    public double Weight { set; get; }
}
