using System.Collections.Generic;
using Newtonsoft.Json;

namespace Maverick.Client.ArangoDb.Protocol;

internal class GetEndpointsFromServerResult
{
    public int Code { get; set; }

    [JsonProperty("endpoints")]
    public IList<Endpoint> Endpoints { get; set; }

    public bool Error { get; set; }
}
