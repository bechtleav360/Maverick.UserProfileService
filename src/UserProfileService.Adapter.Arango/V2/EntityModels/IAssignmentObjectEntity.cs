using System.Collections.Generic;
using Maverick.Client.ArangoDb.Public;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Models;
using Newtonsoft.Json;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

/// <summary>
///     Represents an entity profiles can be assigned to (i.e. a function or a role).
/// </summary>
public interface IAssignmentObjectEntity : IAssignmentObject
{
    /// <summary>
    ///     A list of range-condition settings valid for this Conditional. If it is empty or <c>null</c>, the membership is
    ///     always active.
    /// </summary>
    public IList<RangeCondition> Conditions { get; set; }

    /// <summary>
    ///     A system specific id for this object.
    /// </summary>
    [JsonProperty(AConstants.IdSystemProperty)]
    string SystemId { get; }
}
