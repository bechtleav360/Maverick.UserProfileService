using Newtonsoft.Json;

namespace Maverick.Client.ArangoDb.Protocol;

internal class Endpoint
{
    [JsonProperty("endpoint")]
    public string EndpointAsString { get; set; }
}
