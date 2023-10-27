using Maverick.Client.ArangoDb.Public;
using Newtonsoft.Json;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

internal class ArangoIdentifier
{
    [JsonProperty("collection")]
    public string CollectionName { get; set; }

    [JsonProperty("key")]
    public string Key { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{CollectionName}{AConstants.DocumentHandleSeparator}{Key}";
    }
}
