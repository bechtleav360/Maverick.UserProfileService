using System.Collections.Generic;
using Maverick.Client.ArangoDb.Public;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Models;
using Newtonsoft.Json;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

/// <inheritdoc cref="IContainerProfileEntityModel"/>
public class OrganizationEntityModel : Organization, IContainerProfileEntityModel
{
    /// <summary>
    ///     A list of range-condition settings valid for this <see cref="Member" />. If it is empty or <c>null</c>, the
    ///     membership of this <see cref="Member" /> is always active.
    /// </summary>
    public IList<RangeCondition> Conditions { get; set; }

    /// <summary>
    ///     Gets or sets a collection of functional access rights associated with this organization.
    /// </summary>
    public IList<FunctionalAccessRightEntityModel> FunctionalAccessRights { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this organization has any children.
    /// </summary>
    public bool HasChildren { get; set; }

    /// <summary>
    ///     Gets or sets a collection of containers (e.g. groups, organizations)
    ///     this organization is assigned to.
    /// </summary>
    public new IList<IContainerProfile> MemberOf { set; get; } = new List<IContainerProfile>();

    /// <inheritdoc />
    public IList<string> Paths { get; set; }

    /// <inheritdoc />
    public IList<ILinkedObject> SecurityAssignments { get; set; }

    /// <inheritdoc />
    [JsonProperty(AConstants.IdSystemProperty)]
    public string SystemId { get; set; }

    /// <inheritdoc />
    public List<CalculatedTag> Tags { get; set; }
}
