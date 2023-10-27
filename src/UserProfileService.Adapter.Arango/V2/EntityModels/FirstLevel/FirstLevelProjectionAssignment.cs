using System.Collections.Generic;
using Maverick.Client.ArangoDb.Public;
using Maverick.UserProfileService.Models.Models;
using Newtonsoft.Json;

namespace UserProfileService.Adapter.Arango.V2.EntityModels.FirstLevel;

/// <summary>
///     Contains all data of a profile assignment used in a first-level-projection.
/// </summary>
public class FirstLevelProjectionAssignment
{
    /// <summary>
    ///     The key property used by ArangoDb to identify an object in a collection.
    /// </summary>
    [JsonProperty(AConstants.KeySystemProperty)]
    public string ArangoKey { get; set; }

    /// <summary>
    ///     Conditions of an assignment - they limit the time when a assignment is valid.
    /// </summary>
    public IList<RangeCondition> Conditions { get; set; }
}
