using System.Collections.Generic;
using Maverick.Client.ArangoDb.Public;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Newtonsoft.Json;
using UserProfileService.Adapter.Arango.V2.Annotations;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

/// <inheritdoc cref="IProfileEntityModel"/>
public class UserEntityModel : User, IProfileEntityModel
{
    /// <summary>
    ///     A list of range-condition settings valid for this <see cref="Member" />. If it is empty or <c>null</c>, the
    ///     membership of this <see cref="Member" /> is always active.
    /// </summary>
    public IList<RangeCondition> Conditions { get; set; }

    /// <summary>
    ///     Gets or sets a collection of functional access rights assigned to this user.
    /// </summary>
    public IList<FunctionalAccessRightEntityModel> FunctionalAccessRights { get; set; }

    /// <summary>
    ///     Gets or sets a list of functions assigned to this profile.
    /// </summary>
    [VirtualProperty(
        typeof(UserEntityModel),
        nameof(SecurityAssignments),
        nameof(ILinkedObject.Type),
        nameof(RoleType.Function))]
    [JsonIgnore]
    public IList<ILinkedObject> Functions { get; set; }

    /// <inheritdoc />
    public new IList<Member> MemberOf { get; set; }

    /// <inheritdoc />
    public IList<string> Paths { get; set; }

    /// <inheritdoc />
    public IList<ILinkedObject> SecurityAssignments { get; set; }

    /// <inheritdoc />
    [JsonProperty(AConstants.IdSystemProperty)]
    public string SystemId { get; }

    /// <inheritdoc />
    public List<CalculatedTag> Tags { get; set; }
}
