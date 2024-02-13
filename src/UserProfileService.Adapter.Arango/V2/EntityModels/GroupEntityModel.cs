using System.Collections.Generic;
using Maverick.Client.ArangoDb.Public;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Models;
using Newtonsoft.Json;
using UserProfileService.Adapter.Arango.V2.Annotations;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

/// <summary>
///     Represents an entity model for group profiles.
/// </summary>
public class GroupEntityModel : Group, IContainerProfileEntityModel
{
    /// <summary>
    ///     Gets or sets the number of child profiles assigned to this profile.  
    /// </summary>
    [VirtualProperty(
        typeof(GroupEntityModel),
        nameof(Members),
        typeof(GroupChildrenCountVirtualPropertyResolver))]
    [JsonIgnore]
    public int ChildrenCount { get; set; }

    /// <summary>
    ///     A list of range-condition settings valid for this <see cref="Member" />. If it is empty or <c>null</c>, the
    ///     membership of this <see cref="Member" /> is always active.
    /// </summary>
    public IList<RangeCondition> Conditions { get; set; }

    /// <summary>
    ///     A list of functional access rights for this group.
    /// </summary>
    public IList<FunctionalAccessRightEntityModel> FunctionalAccessRights { get; set; }

    /// <summary>
    ///     Gets or sets whether this group has any children.
    /// </summary>
    public bool HasChildren { get; set; }

    /// <inheritdoc />
    public new IList<Member> MemberOf { set; get; } = new List<Member>();

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
