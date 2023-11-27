using System.Collections.Generic;
using Maverick.Client.ArangoDb.Public;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Models;
using Newtonsoft.Json;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

public interface IProfileEntityModel : IProfile, ITagsIncludedObject
{
    /// <summary>
    ///     A list of range-condition settings valid for this <see cref="Member" />. If it is empty or <c>null</c>, the
    ///     membership of this <see cref="Member" /> is always active.
    /// </summary>
    public IList<RangeCondition> Conditions { get; set; }

    IList<Member> MemberOf { set; get; }
    public List<string> Paths { get; set; }
    IList<ILinkedObject> SecurityAssignments { get; set; }

    [JsonProperty(AConstants.IdSystemProperty)]
    string SystemId { get; }
}
