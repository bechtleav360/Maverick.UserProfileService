using System.Collections.Generic;
using Maverick.Client.ArangoDb.Public;
using Maverick.UserProfileService.Models.Models;
using Newtonsoft.Json;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

internal class FunctionObjectEntityModel : FunctionView, IAssignmentObjectEntity
{
    ///<inheritdoc />
    public IList<RangeCondition> Conditions { get; set; }

    [JsonProperty(AConstants.IdSystemProperty)]
    public string SystemId { get; }
}
