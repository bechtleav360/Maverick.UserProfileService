using System.Collections.Generic;
using Maverick.Client.ArangoDb.Public;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Newtonsoft.Json;
using UserProfileService.Adapter.Arango.V2.Annotations;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

internal class UserEntityModel : User, IProfileEntityModel
{
    /// <summary>
    ///     A list of range-condition settings valid for this <see cref="Member" />. If it is empty or <c>null</c>, the
    ///     membership of this <see cref="Member" /> is always active.
    /// </summary>
    public IList<RangeCondition> Conditions { get; set; }

    public List<FunctionalAccessRightEntityModel> FunctionalAccessRights { get; set; }

    [VirtualProperty(
        typeof(UserEntityModel),
        nameof(SecurityAssignments),
        nameof(ILinkedObject.Type),
        nameof(RoleType.Function))]
    [JsonIgnore]
    public IList<ILinkedObject> Functions { get; set; }

    public new IList<Member> MemberOf { get; set; }

    public List<string> Paths { get; set; }

    public IList<ILinkedObject> SecurityAssignments { get; set; }

    [JsonProperty(AConstants.IdSystemProperty)]
    public string SystemId { get; }

    public List<CalculatedTag> Tags { get; set; }
}
