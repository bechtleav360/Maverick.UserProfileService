using System.Collections.Generic;
using Maverick.Client.ArangoDb.Public;
using Maverick.UserProfileService.Models.Models;
using Newtonsoft.Json;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

/// <summary>
///     Represents an entity model for function objects.
/// </summary>
public class FunctionObjectEntityModel : FunctionView, IAssignmentObjectEntity
{
    ///<inheritdoc />
    public IList<RangeCondition> Conditions { get; set; }

    /// <inheritdoc />
    [JsonProperty(AConstants.IdSystemProperty)]
    public string SystemId { get; }
}
