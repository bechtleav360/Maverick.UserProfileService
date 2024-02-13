using System.Collections.Generic;
using Maverick.Client.ArangoDb.Public;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Models;
using Newtonsoft.Json;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

/// <summary>
///     Represents a profile entity model.
/// </summary>
public interface IProfileEntityModel : IProfile, ITagsIncludedObject
{
    /// <summary>
    ///     A list of range-condition settings valid for this <see cref="Member" />. If it is empty or <c>null</c>, the
    ///     membership of this <see cref="Member" /> is always active.
    /// </summary>
    public IList<RangeCondition> Conditions { get; set; }

    /// <summary>
    ///     Gets or sets a list of profiles that this profile is a member of.
    /// </summary>
    IList<Member> MemberOf { set; get; }

    /// <summary>
    ///     Gets or sets a list of paths that lead to this profile.
    /// </summary>
    public IList<string> Paths { get; set; }

    /// <summary>
    ///     Gets or sets a list of linked objects (i.e. functions, roles) that this profile is assigned to.
    /// </summary>
    IList<ILinkedObject> SecurityAssignments { get; set; }

    /// <summary>
    ///     A system specific id for this object.
    /// </summary>
    [JsonProperty(AConstants.IdSystemProperty)]
    string SystemId { get; }
}
