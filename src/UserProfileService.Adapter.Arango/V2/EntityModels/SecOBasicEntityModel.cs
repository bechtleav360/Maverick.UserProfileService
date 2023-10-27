using System.Collections.Generic;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.Models;
using Newtonsoft.Json;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

internal class SecOBasicEntityModel : SecOBasic
{
    [JsonProperty("_id")]
    public string SystemId { get; set; }

    public new IList<CalculatedTag> Tags { set; get; } = new List<CalculatedTag>();
}
