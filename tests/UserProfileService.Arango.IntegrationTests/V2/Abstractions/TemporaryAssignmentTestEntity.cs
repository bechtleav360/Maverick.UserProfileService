using Newtonsoft.Json;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Arango.IntegrationTests.V2.Abstractions
{
    public class TemporaryAssignmentTestEntity : FirstLevelProjectionTemporaryAssignment
    {
        [JsonProperty(nameof(CompoundKey))]
        public string StoredCompoundKey { get; set; }
    }
}
