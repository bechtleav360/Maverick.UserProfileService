using System.Collections.Generic;
using Maverick.Client.ArangoDb.Public;
using Newtonsoft.Json;

namespace UserProfileService.Arango.IntegrationTests.V2.Abstractions
{
    public class EdgeModel
    {
        [JsonProperty(AConstants.SystemPropertyFrom)]
        public string From { get; set; }

        [JsonProperty(AConstants.SystemPropertyTo)]
        public string To { get; set; }

        [JsonExtensionData]
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        public EdgeModel(string from, string fromCollection, string to, string toCollection)
        {
            From = $"{fromCollection}/{from}";
            To = $"{toCollection}/{to}";
        }
    }
}
