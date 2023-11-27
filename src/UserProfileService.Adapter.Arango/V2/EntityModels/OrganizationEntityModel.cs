using System.Collections.Generic;
using Maverick.Client.ArangoDb.Public;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Models;
using Newtonsoft.Json;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

public class OrganizationEntityModel : Organization, IContainerProfileEntityModel
{
    /// <summary>
    ///     A list of range-condition settings valid for this <see cref="Member" />. If it is empty or <c>null</c>, the
    ///     membership of this <see cref="Member" /> is always active.
    /// </summary>
    public IList<RangeCondition> Conditions { get; set; }

    public List<FunctionalAccessRightEntityModel> FunctionalAccessRights { get; set; }
    public bool HasChildren { get; set; }

    public new IList<IContainerProfile> MemberOf { set; get; } = new List<IContainerProfile>();
    public List<string> Paths { get; set; }

    public IList<ILinkedObject> SecurityAssignments { get; set; }

    [JsonProperty(AConstants.IdSystemProperty)]
    public string SystemId { get; set; }

    public List<CalculatedTag> Tags { get; set; }
}
